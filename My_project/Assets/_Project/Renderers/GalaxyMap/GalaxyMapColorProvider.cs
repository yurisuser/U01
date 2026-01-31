using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;
using UnityEngine;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    /// <summary>Общий расчёт цветов для звёзд/линков (созвездия и фракции).</summary>
    internal static class GalaxyMapColorProvider
    {
        public static Color[] BuildConstellationColors(
            StarSys[] systems,
            bool useFractionColoring,
            float constellationSaturation,
            float constellationValue,
            float constellationAlpha,
            Color emptyConstellationColor)
        {
            if (systems == null || systems.Length == 0)
                return null;

            int maxId = 0;
            for (int i = 0; i < systems.Length; i++)
            {
                int cid = systems[i].ConstellationId;
                if (cid > maxId)
                    maxId = cid;
            }

            if (maxId <= 0)
                return null;

            var colors = new Color[maxId + 1];
            var hasOwnerColor = new bool[maxId + 1];
            var adjacency = BuildConstellationAdjacency(systems);

            for (int i = 1; i <= maxId; i++)
            {
                float hue = Hash01(i);
                var baseColor = Color.HSVToRGB(hue, constellationSaturation, constellationValue);
                baseColor.a = constellationAlpha;
                colors[i] = baseColor;
            }

            EnsureConstellationColorSeparation(colors, adjacency);

            for (int i = 0; i < systems.Length; i++)
            {
                int cid = systems[i].ConstellationId;
                if (cid <= 0 || cid >= colors.Length || hasOwnerColor[cid])
                    continue;

                var owner = systems[i].OwnerFrac;
                if (owner == null || owner.Id <= 0)
                    continue;

                if (useFractionColoring && TryGetFractionColor(owner, constellationAlpha, out var ownerColor))
                {
                    colors[cid] = ownerColor;
                    hasOwnerColor[cid] = true;
                }
            }

            return colors;
        }

        public static Color GetSystemColor(
            StarSys[] systems,
            int systemIndex,
            bool useHyperlinkColoring,
            bool useFractionColoring,
            Color[] constellationColors,
            float constellationSaturation,
            float constellationValue,
            float constellationAlpha,
            Color emptyConstellationColor,
            Color noColoringColor,
            Color defaultColor)
        {
            if (systems == null || systemIndex < 0 || systemIndex >= systems.Length)
                return defaultColor;

            if (!useHyperlinkColoring && !useFractionColoring)
                return noColoringColor;

            if (useFractionColoring && TryGetSystemFractionColor(systems, systemIndex, constellationAlpha, out var fracColor))
                return fracColor;

            if (useHyperlinkColoring)
                return GetConstellationColor(constellationColors, systems[systemIndex].ConstellationId, constellationSaturation, constellationValue, constellationAlpha, defaultColor, emptyConstellationColor);

            return noColoringColor;
        }

        public static bool TryGetSystemFractionColor(StarSys[] systems, int systemIndex, float constellationAlpha, out Color color)
        {
            color = default;
            if (systems == null || systemIndex < 0 || systemIndex >= systems.Length)
                return false;

            var owner = systems[systemIndex].OwnerFrac;
            if (owner == null || owner.Id <= 0)
                return false;

            if (!TryGetFractionColor(owner, constellationAlpha, out var parsed))
                return false;

            color = parsed;
            return true;
        }

        public static bool TryGetFractionColor(in Fraction fraction, float alpha, out Color color)
        {
            color = default;
            if (string.IsNullOrWhiteSpace(fraction.Color))
                return false;

            if (!ColorUtility.TryParseHtmlString(fraction.Color, out var parsed))
                return false;

            parsed.a = alpha;
            color = parsed;
            return true;
        }

        public static Color GetConstellationColor(
            Color[] constellationColors,
            int constellationId,
            float constellationSaturation,
            float constellationValue,
            float constellationAlpha,
            Color defaultColor,
            Color emptyConstellationColor)
        {
            if (constellationId <= 0)
            {
                var empty = emptyConstellationColor;
                empty.a = constellationAlpha;
                return empty;
            }

            if (constellationColors == null || constellationId >= constellationColors.Length)
            {
                float hue = Hash01(constellationId);
                var fallback = Color.HSVToRGB(hue, constellationSaturation, constellationValue);
                fallback.a = constellationAlpha;
                return fallback;
            }

            var color = constellationColors[constellationId];
            if (color.a <= 0f)
            {
                var empty = emptyConstellationColor;
                empty.a = constellationAlpha;
                return empty;
            }

            return color;
        }

        private static Dictionary<int, HashSet<int>> BuildConstellationAdjacency(StarSys[] systems)
        {
            var adjacency = new Dictionary<int, HashSet<int>>();
            if (systems == null || systems.Length == 0)
                return adjacency;

            var seenEdges = new HashSet<long>();
            for (int i = 0; i < systems.Length; i++)
            {
                var links = systems[i].links;
                if (links == null || links.Length == 0)
                    continue;

                int cidA = systems[i].ConstellationId;
                if (cidA <= 0)
                    continue;

                for (int j = 0; j < links.Length; j++)
                {
                    int other = links[j];
                    if (other < 0 || other >= systems.Length)
                        continue;

                    long key = GetEdgeKey(i, other);
                    if (!seenEdges.Add(key))
                        continue;

                    int cidB = systems[other].ConstellationId;
                    if (cidB <= 0 || cidA == cidB)
                        continue;

                    if (!adjacency.TryGetValue(cidA, out var setA))
                    {
                        setA = new HashSet<int>();
                        adjacency[cidA] = setA;
                    }
                    setA.Add(cidB);

                    if (!adjacency.TryGetValue(cidB, out var setB))
                    {
                        setB = new HashSet<int>();
                        adjacency[cidB] = setB;
                    }
                    setB.Add(cidA);
                }
            }

            return adjacency;
        }

        private static void EnsureConstellationColorSeparation(Color[] colors, Dictionary<int, HashSet<int>> adjacency)
        {
            if (colors == null || colors.Length == 0 || adjacency == null || adjacency.Count == 0)
                return;

            const float minHueDelta = 0.08f; // ~29 deg
            const float hueStep = 0.381966f; // golden angle fraction
            const int maxAttempts = 8;

            for (int cid = 1; cid < colors.Length; cid++)
            {
                if (!adjacency.TryGetValue(cid, out var neighbors) || neighbors == null || neighbors.Count == 0)
                    continue;

                var color = colors[cid];
                Color.RGBToHSV(color, out var hue, out var sat, out var val);

                int attempts = 0;
                while (attempts < maxAttempts)
                {
                    bool tooClose = false;
                    foreach (var neighborCid in neighbors)
                    {
                        if (neighborCid <= 0 || neighborCid >= colors.Length)
                            continue;

                        Color.RGBToHSV(colors[neighborCid], out var nhue, out _, out _);
                        float diff = Mathf.Abs(Mathf.DeltaAngle(hue * 360f, nhue * 360f)) / 360f;
                        if (diff < minHueDelta)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                        break;

                    hue = Mathf.Repeat(hue + hueStep, 1f);
                    attempts++;
                }

                var adjusted = Color.HSVToRGB(hue, sat, val);
                adjusted.a = color.a;
                colors[cid] = adjusted;
            }
        }

        private static long GetEdgeKey(int a, int b)
        {
            int min = a < b ? a : b;
            int max = a < b ? b : a;
            return ((long)min << 32) | (uint)max;
        }

        private static float Hash01(int seed)
        {
            unchecked
            {
                uint x = (uint)seed;
                x ^= x >> 17;
                x *= 0xED5AD4BBu;
                x ^= x >> 11;
                x *= 0xAC4C1B51u;
                x ^= x >> 15;
                x *= 0x31848BABu;
                x ^= x >> 14;
                return (x & 0xFFFFFFu) / 16777216f;
            }
        }
    }
}
