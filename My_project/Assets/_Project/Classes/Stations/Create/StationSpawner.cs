using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Config;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.Stations
{
    /// <summary>Расставляет станции в системе с учётом планет и границ орбит.</summary>
    public static class StationSpawner
    {
        /// <summary>Создаёт одну станцию в системе для указанной фракции и дефиниции.</summary>
        public static Station CreateForSystem(in StarSys sys, StationTypeDef def, Fraction owner)
        {
            Vector3 position = FindPosition(in sys);
            return StationCreator.Create(def, owner, position);
        }

        private static Vector3 FindPosition(in StarSys sys)
        {
            float orbitUnit = OrbitMath.PlanetOrbitIndexToUnits(1);
            float innerRadius = Mathf.Max(0f, StarSysemConstants.InnerDeadZoneOrbits * orbitUnit);
            float outerRadius = OrbitMath.PlanetOrbitIndexToUnits(StarSysemConstants.OrbitSlots);

            if (outerRadius <= 0f || outerRadius <= innerRadius)
                return Vector3.zero;

            float safeOuter = Mathf.Max(innerRadius + orbitUnit, outerRadius - orbitUnit * 0.5f); // чуть внутри 20-й орбиты
            float targetRadius = Mathf.Lerp(innerRadius, safeOuter, 0.85f); // ближе к внешней границе, но с запасом

            var planetPositions = CollectPlanetPositions(in sys, orbitUnit);
            if (planetPositions.Count == 0)
                return new Vector3(targetRadius, 0f, 0f);

            float bestScore = -1f;
            float bestAngle = 0f;

            float angle = SeedAngle(sys.Uid.Id);
            for (int i = 0; i < StarSysemConstants.MaxAngleAttempts; i++)
            {
                Vector3 candidate = AngleToPos(angle, targetRadius);
                float score = ScorePosition(candidate, planetPositions, targetRadius, safeOuter);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestAngle = angle;
                }

                angle += StarSysemConstants.GoldenAngleRad;
            }

            return AngleToPos(bestAngle, targetRadius);
        }

        private static float ScorePosition(in Vector3 candidate, IReadOnlyList<Vector3> others, float radius, float outerRadius)
        {
            float minDistSq = float.MaxValue;
            for (int i = 0; i < others.Count; i++)
            {
                float d = (candidate - others[i]).sqrMagnitude;
                if (d < minDistSq)
                    minDistSq = d;
            }

            float distToOuter = Mathf.Max(0f, outerRadius - radius);
            float distScore = Mathf.Sqrt(minDistSq);
            return Mathf.Min(distScore, distToOuter); // ближе к объектам или границе — хуже
        }

        private static List<Vector3> CollectPlanetPositions(in StarSys sys, float orbitUnit)
        {
            var list = new List<Vector3>();
            if (sys.PlanetSysArr == null || sys.PlanetSysArr.Length == 0)
                return list;

            for (int i = 0; i < sys.PlanetSysArr.Length; i++)
            {
                var p = sys.PlanetSysArr[i];
                float r = Mathf.Max(0, p.OrbitIndex) * orbitUnit;
                float angle = p.OrbitPosition;
                list.Add(new Vector3(Mathf.Cos(angle) * r, Mathf.Sin(angle) * r, 0f));
            }

            return list;
        }

        private static Vector3 AngleToPos(float angle, float radius)
        {
            return new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        private static float SeedAngle(int seed)
        {
            uint x = (uint)seed;
            x ^= x >> 17; x *= 0xED5AD4BBu;
            x ^= x >> 11; x *= 0xAC4C1B51u;
            x ^= x >> 15; x *= 0x31848BABu;
            x ^= x >> 14;
            return (x & 0xFFFFFFu) / 16777216f * Mathf.PI * 2f;
        }
    }
}
