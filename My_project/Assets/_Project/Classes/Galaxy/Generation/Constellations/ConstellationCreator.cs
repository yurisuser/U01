using System;
using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class ConstellationCreator
    {
        // Временные константы для отладки. После отладки перенесём в CONST.
        private const int AngularSegments = 6;
        private const int RadialSegments = 8;
        private const int MinGroupSize = 7;
        private const int MaxGroupSize = 20;
        private const int MinLinksPerStar = 1;
        private const int MaxLinksPerStar = 4;
        private const float LinkDistanceLimit = 20f;

        // Оркестратор: запускает стадии генерации и связывает их между собой.
        public static void Generate(StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return;

            for (int i = 0; i < galaxy.Length; i++)
            {
                ref var sys = ref galaxy[i];
                sys.links = System.Array.Empty<int>();
                sys.ConstellationId = -1;
            }

            var activeStars = CollectActiveStars(galaxy, LinkDistanceLimit);
            var fragments = SegmentStars(activeStars, galaxy, AngularSegments, RadialSegments);
            PickFragmentCenters(fragments);

            var groups = BuildGroups(fragments, galaxy, MinGroupSize, MaxGroupSize, LinkDistanceLimit);
            var links = BuildIntraGroupLinks(groups, galaxy, MinLinksPerStar, MaxLinksPerStar, LinkDistanceLimit);

            var interGroupLinks = BuildInterGroupLinks(groups, galaxy);
            if (interGroupLinks.Count > 0)
                links.AddRange(interGroupLinks);

            ApplyLinks(galaxy, links);

            for (int i = 0; i < groups.Count; i++)
            {
                var group = groups[i];
                for (int j = 0; j < group.StarIndices.Count; j++)
                {
                    int starIndex = group.StarIndices[j];
                    ref var sys = ref galaxy[starIndex];
                    sys.ConstellationId = i;
                }
            }

            Validate(groups, links, MinGroupSize, MaxGroupSize, MinLinksPerStar, MaxLinksPerStar);
        }

        // Первый проход: исключаем звезды без соседки ближе лимита (дистанция по текущим координатам).
        private static List<int> CollectActiveStars(StarSys[] galaxy, float linkDistanceLimit)
        {
            var result = new List<int>(galaxy.Length);
            if (galaxy.Length <= 1)
                return result;

            float limitSq = linkDistanceLimit * linkDistanceLimit;

            for (int i = 1; i < galaxy.Length; i++)
            {
                var pos = galaxy[i].GalaxyPosition;
                bool hasNeighbor = false;

                for (int j = 1; j < galaxy.Length; j++)
                {
                    if (i == j) continue;
                    var other = galaxy[j].GalaxyPosition;
                    float dx = pos.x - other.x;
                    float dy = pos.y - other.y;
                    if (dx * dx + dy * dy <= limitSq)
                    {
                        hasNeighbor = true;
                        break;
                    }
                }

                if (hasNeighbor)
                    result.Add(i);
            }

            return result;
        }

        // Делим активные звезды на угловые сегменты и радиальные кольца.
        private static List<ConstellationFragment> SegmentStars(IReadOnlyList<int> starIndices, StarSys[] galaxy, int angularSegments, int radialSegments)
        {
            var fragments = new List<ConstellationFragment>(angularSegments * radialSegments);
            for (int seg = 0; seg < angularSegments; seg++)
            {
                for (int ring = 0; ring < radialSegments; ring++)
                {
                    fragments.Add(new ConstellationFragment
                    {
                        SegmentIndex = seg,
                        RingIndex = ring
                    });
                }
            }

            if (starIndices.Count == 0)
                return fragments;

            float maxR2 = 0f;
            for (int i = 0; i < starIndices.Count; i++)
            {
                int index = starIndices[i];
                var pos = galaxy[index].GalaxyPosition;
                float r2 = pos.x * pos.x + pos.y * pos.y;
                if (r2 > maxR2) maxR2 = r2;
            }

            if (maxR2 <= 0f)
                maxR2 = 1f;

            float fullAngle = Mathf.PI * 2f;
            for (int i = 0; i < starIndices.Count; i++)
            {
                int starIndex = starIndices[i];
                var pos = galaxy[starIndex].GalaxyPosition;
                float r2 = pos.x * pos.x + pos.y * pos.y;

                int ringIndex = (int)((r2 / maxR2) * radialSegments);
                if (ringIndex >= radialSegments)
                    ringIndex = radialSegments - 1;

                float angle = Mathf.Atan2(pos.y, pos.x);
                if (angle < 0f) angle += fullAngle;

                int segmentIndex = (int)((angle / fullAngle) * angularSegments);
                if (segmentIndex >= angularSegments)
                    segmentIndex = angularSegments - 1;

                int fragmentIndex = segmentIndex * radialSegments + ringIndex;
                fragments[fragmentIndex].StarIndices.Add(starIndex);
            }

            return fragments;
        }

        // Для каждого фрагмента выбираем случайную звезду как центр.
        private static void PickFragmentCenters(List<ConstellationFragment> fragments)
        {
            for (int i = 0; i < fragments.Count; i++)
            {
                var fragment = fragments[i];
                if (fragment.StarIndices.Count == 0)
                    continue;

                int pick = UnityEngine.Random.Range(0, fragment.StarIndices.Count);
                fragment.CenterStarIndex = fragment.StarIndices[pick];
            }
        }

        // Собираем группы вокруг центров с ограничением дистанции и размером 7–20.
        private static List<ConstellationGroup> BuildGroups(List<ConstellationFragment> fragments, StarSys[] galaxy, int minGroupSize, int maxGroupSize, float linkDistanceLimit)
        {
            var groups = new List<ConstellationGroup>(fragments.Count);
            float limitSq = linkDistanceLimit * linkDistanceLimit;

            for (int i = 0; i < fragments.Count; i++)
            {
                var fragment = fragments[i];
                if (fragment.StarIndices.Count == 0)
                    continue;

                int centerIndex = fragment.CenterStarIndex;
                if (centerIndex <= 0)
                    centerIndex = fragment.StarIndices[0];

                var candidates = new List<StarDistance>(fragment.StarIndices.Count);
                for (int j = 0; j < fragment.StarIndices.Count; j++)
                {
                    int starIndex = fragment.StarIndices[j];
                    float distSq = DistanceSq(galaxy, centerIndex, starIndex);
                    if (distSq <= limitSq)
                    {
                        candidates.Add(new StarDistance
                        {
                            Index = starIndex,
                            DistSq = distSq
                        });
                    }
                }

                if (candidates.Count == 0)
                    continue;

                candidates.Sort(StarDistanceComparer);

                var group = new ConstellationGroup
                {
                    CenterStarIndex = centerIndex,
                    SegmentIndex = fragment.SegmentIndex,
                    RingIndex = fragment.RingIndex
                };

                int countToTake = Mathf.Min(maxGroupSize, candidates.Count);
                for (int j = 0; j < countToTake; j++)
                    group.StarIndices.Add(candidates[j].Index);

                groups.Add(group);
            }

            return groups;
        }

        // Внутри группы: 1–4 линка на звезду, с ограничением по длине.
        private static List<ConstellationLinkEdge> BuildIntraGroupLinks(List<ConstellationGroup> groups, StarSys[] galaxy, int minLinksPerStar, int maxLinksPerStar, float linkDistanceLimit)
        {
            var links = new List<ConstellationLinkEdge>();
            var edgeSet = new HashSet<long>();
            float limitSq = linkDistanceLimit * linkDistanceLimit;

            for (int g = 0; g < groups.Count; g++)
            {
                var group = groups[g];
                int count = group.StarIndices.Count;
                if (count < 2)
                    continue;

                var indexMap = new int[galaxy.Length];
                for (int i = 0; i < indexMap.Length; i++)
                    indexMap[i] = -1;

                for (int i = 0; i < count; i++)
                    indexMap[group.StarIndices[i]] = i;

                var candidates = new List<int>[count];
                for (int i = 0; i < count; i++)
                {
                    int starIndex = group.StarIndices[i];
                    var list = new List<StarDistance>(count);

                    for (int j = 0; j < count; j++)
                    {
                        if (i == j) continue;
                        int otherIndex = group.StarIndices[j];
                        float distSq = DistanceSq(galaxy, starIndex, otherIndex);
                        if (distSq <= limitSq)
                        {
                            list.Add(new StarDistance
                            {
                                Index = otherIndex,
                                DistSq = distSq
                            });
                        }
                    }

                    list.Sort(StarDistanceComparer);
                    var ordered = new List<int>(list.Count);
                    for (int j = 0; j < list.Count; j++)
                        ordered.Add(list[j].Index);

                    candidates[i] = ordered;
                }

                var degrees = new int[count];
                var targets = new int[count];
                for (int i = 0; i < count; i++)
                    targets[i] = UnityEngine.Random.Range(minLinksPerStar, maxLinksPerStar + 1);

                for (int i = 0; i < count; i++)
                {
                    int starIndex = group.StarIndices[i];
                    while (degrees[i] < targets[i])
                    {
                        bool added = false;
                        var candidateList = candidates[i];
                        for (int j = 0; j < candidateList.Count; j++)
                        {
                            int otherIndex = candidateList[j];
                            int otherPos = indexMap[otherIndex];
                            if (otherPos < 0)
                                continue;

                            if (degrees[otherPos] >= targets[otherPos])
                                continue;

                            long key = GetEdgeKey(starIndex, otherIndex);
                            if (edgeSet.Contains(key))
                                continue;

                            edgeSet.Add(key);
                            links.Add(new ConstellationLinkEdge(starIndex, otherIndex));
                            degrees[i]++;
                            degrees[otherPos]++;
                            added = true;
                            break;
                        }

                        if (!added)
                            break;
                    }
                }
            }

            return links;
        }

        // Между соседними созвездиями: один гиперпереход.
        private static List<ConstellationLinkEdge> BuildInterGroupLinks(List<ConstellationGroup> groups, StarSys[] galaxy)
        {
            var links = new List<ConstellationLinkEdge>();
            if (groups.Count < 2)
                return links;

            var edgeSet = new HashSet<long>();
            float limitSq = LinkDistanceLimit * LinkDistanceLimit;

            for (int i = 0; i < groups.Count; i++)
            {
                int a = groups[i].CenterStarIndex;
                if (a <= 0)
                    continue;

                float bestDist = float.MaxValue;
                int bestIndex = -1;

                for (int j = 0; j < groups.Count; j++)
                {
                    if (i == j) continue;
                    int b = groups[j].CenterStarIndex;
                    if (b <= 0) continue;

                    float distSq = DistanceSq(galaxy, a, b);
                    if (distSq < bestDist)
                    {
                        bestDist = distSq;
                        bestIndex = b;
                    }
                }

                if (bestIndex < 0 || bestDist > limitSq)
                    continue;

                long key = GetEdgeKey(a, bestIndex);
                if (edgeSet.Contains(key))
                    continue;

                edgeSet.Add(key);
                links.Add(new ConstellationLinkEdge(a, bestIndex));
            }

            return links;
        }

        // Записываем линки в StarSys.
        private static void ApplyLinks(StarSys[] galaxy, List<ConstellationLinkEdge> links)
        {
            var linkLists = new List<int>[galaxy.Length];

            for (int i = 0; i < links.Count; i++)
            {
                int a = links[i].A;
                int b = links[i].B;

                if (a < 0 || b < 0 || a >= galaxy.Length || b >= galaxy.Length)
                    continue;

                if (linkLists[a] == null)
                    linkLists[a] = new List<int>();
                if (linkLists[b] == null)
                    linkLists[b] = new List<int>();

                linkLists[a].Add(b);
                linkLists[b].Add(a);
            }

            for (int i = 0; i < galaxy.Length; i++)
            {
                ref var sys = ref galaxy[i];
                sys.links = linkLists[i] == null
                    ? System.Array.Empty<int>()
                    : linkLists[i].ToArray();
            }
        }

        // Валидация размеров групп и степени звёзд.
        private static void Validate(List<ConstellationGroup> groups, List<ConstellationLinkEdge> links, int minGroupSize, int maxGroupSize, int minLinksPerStar, int maxLinksPerStar)
        {
            if (groups == null || links == null)
                return;

            // Пока только базовые проверки, без исправлений.
            for (int i = 0; i < groups.Count; i++)
            {
                int count = groups[i].StarIndices.Count;
                if (count < minGroupSize || count > maxGroupSize)
                {
                    // Здесь можно будет добавить реакцию на неверный размер.
                }
            }
        }

        private static float DistanceSq(StarSys[] galaxy, int a, int b)
        {
            var aPos = galaxy[a].GalaxyPosition;
            var bPos = galaxy[b].GalaxyPosition;
            float dx = aPos.x - bPos.x;
            float dy = aPos.y - bPos.y;
            return dx * dx + dy * dy;
        }

        private static long GetEdgeKey(int a, int b)
        {
            int min = a < b ? a : b;
            int max = a < b ? b : a;
            return ((long)min << 32) | (uint)max;
        }

        private struct StarDistance
        {
            public int Index;
            public float DistSq;
        }

        private static readonly System.Comparison<StarDistance> StarDistanceComparer = (a, b) =>
        {
            if (a.DistSq < b.DistSq) return -1;
            if (a.DistSq > b.DistSq) return 1;
            return 0;
        };
    }
}
