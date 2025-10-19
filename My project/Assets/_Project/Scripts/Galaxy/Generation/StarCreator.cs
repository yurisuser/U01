using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.ID;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class StarCreator
    {
        // === веса типов звёзд (0–100) ===
        private static readonly int[] TypeWeights = {
            20, // Red
            20, // Orange
            10,  // Yellow
            7, // White
            3,  // Blue
            2,  // Neutron
            1   // Black
        };

        // === веса размеров звёзд по типу (в процентах суммарно 100) ===
        private static readonly int[] RedSizeWeights     = { 85, 14, 1, 0 };
        private static readonly int[] OrangeSizeWeights  = { 65, 30, 5, 0 };
        private static readonly int[] YellowSizeWeights  = { 40, 50, 9, 1 };
        private static readonly int[] WhiteSizeWeights   = { 20, 55, 22, 3 };
        private static readonly int[] BlueSizeWeights    = { 0, 25, 45, 30 };
        private static readonly int[] NeutronSizeWeights = { 100, 0, 0, 0 };
        private static readonly int[] BlackSizeWeights   = { 0, 10, 45, 45 };

        // === диапазоны физических параметров ===
        private static readonly Vector2 TempKRed     = new(2400f, 3600f);
        private static readonly Vector2 TempKOrange  = new(3600f, 5200f);
        private static readonly Vector2 TempKYellow  = new(5200f, 6000f);
        private static readonly Vector2 TempKWhite   = new(6500f, 10000f);
        private static readonly Vector2 TempKBlue    = new(10000f, 30000f);
        private static readonly Vector2 TempKNeutron = new(6e5f, 1e6f);
        private static readonly Vector2 TempKBlack   = new(2f, 10f);

        private static readonly Vector2 MassRed     = new(0.1f, 0.7f);
        private static readonly Vector2 MassOrange  = new(0.7f, 0.9f);
        private static readonly Vector2 MassYellow  = new(0.9f, 1.2f);
        private static readonly Vector2 MassWhite   = new(1.2f, 2.5f);
        private static readonly Vector2 MassBlue    = new(5f, 40f);
        private static readonly Vector2 MassNeutron = new(1.1f, 2.3f);
        private static readonly Vector2 MassBlack   = new(5f, 30f);

        private static readonly Vector2 RadRed     = new(0.1f, 0.7f);
        private static readonly Vector2 RadOrange  = new(0.7f, 0.9f);
        private static readonly Vector2 RadYellow  = new(0.9f, 1.2f);
        private static readonly Vector2 RadWhite   = new(1.2f, 2.0f);
        private static readonly Vector2 RadBlue    = new(3f, 10f);
        private static readonly Vector2 RadNeutron = new(1e-5f, 2e-5f);
        private static readonly Vector2 RadBlack   = new(1e-5f, 1e-3f);

        // множители размеров
        private const float MulDwarf      = 0.7f;
        private const float MulNormal     = 1.0f;
        private const float MulGiant      = 10f;
        private const float MulSupergiant = 50f;

        // === Публичный API ===
        public static Star Create()
        {
            var type = PickStarTypeWeighted();
            return Create(type);
        }

        public static Star Create(EStarType forcedType)
        {
            var size = PickStarSizeWeighted(forcedType);
            return BuildStar(forcedType, size);
        }

        // === внутренние методы ===
        private static Star BuildStar(EStarType type, EStarSize size)
        {
            float temperature = PickTemp(type);
            float mass = PickByType(type, MassRed, MassOrange, MassYellow, MassWhite, MassBlue, MassNeutron, MassBlack);
            float radius = PickByType(type, RadRed, RadOrange, RadYellow, RadWhite, RadBlue, RadNeutron, RadBlack) * SizeMultiplier(size);
            float luminosity = Mathf.Clamp(Mathf.Pow(mass, 3.5f), 0.001f, 1e6f);

            float age = Random.Range(0.1f, 12f);
            float metallicity = Random.Range(0.0f, 1.0f);
            float stability = Mathf.Clamp01(1f - (mass / 40f) + Random.Range(-0.1f, 0.1f));

            return new Star
            {
                UID = IDService.Create(EntityType.Star),
                name = null,
                type = type,
                size = size,
                temperature = temperature,
                mass = mass,
                radius = radius,
                luminosity = luminosity,
                age = age,
                metallicity = metallicity,
                stability = stability
            };
        }

        private static EStarType PickStarTypeWeighted()
        {
            int total = 0;
            for (int i = 0; i < TypeWeights.Length; i++) total += TypeWeights[i];
            int r = Random.Range(0, total);
            int acc = 0;
            for (int i = 0; i < TypeWeights.Length; i++)
            {
                acc += TypeWeights[i];
                if (r < acc) return (EStarType)i;
            }
            return EStarType.Red;
        }

        private static EStarSize PickStarSizeWeighted(EStarType t)
        {
            int[] w = t switch
            {
                EStarType.Red     => RedSizeWeights,
                EStarType.Orange  => OrangeSizeWeights,
                EStarType.Yellow  => YellowSizeWeights,
                EStarType.White   => WhiteSizeWeights,
                EStarType.Blue    => BlueSizeWeights,
                EStarType.Neutron => NeutronSizeWeights,
                EStarType.Black   => BlackSizeWeights,
                _ => YellowSizeWeights
            };

            int total = w[0] + w[1] + w[2] + w[3];
            int r = Random.Range(0, total);
            if (r < w[0]) return EStarSize.Dwarf;
            r -= w[0];
            if (r < w[1]) return EStarSize.Normal;
            r -= w[1];
            if (r < w[2]) return EStarSize.Giant;
            return EStarSize.Supergiant;
        }

        private static float PickTemp(EStarType t)
        {
            var range = t switch
            {
                EStarType.Red     => TempKRed,
                EStarType.Orange  => TempKOrange,
                EStarType.Yellow  => TempKYellow,
                EStarType.White   => TempKWhite,
                EStarType.Blue    => TempKBlue,
                EStarType.Neutron => TempKNeutron,
                EStarType.Black   => TempKBlack,
                _ => TempKYellow
            };
            return Random.Range(range.x, range.y);
        }

        private static float PickByType(EStarType t, Vector2 red, Vector2 orange, Vector2 yellow, Vector2 white, Vector2 blue, Vector2 neutron, Vector2 black)
        {
            Vector2 range = t switch
            {
                EStarType.Red     => red,
                EStarType.Orange  => orange,
                EStarType.Yellow  => yellow,
                EStarType.White   => white,
                EStarType.Blue    => blue,
                EStarType.Neutron => neutron,
                EStarType.Black   => black,
                _ => yellow
            };
            return Random.Range(range.x, range.y);
        }

        private static float SizeMultiplier(EStarSize s) => s switch
        {
            EStarSize.Dwarf      => MulDwarf,
            EStarSize.Normal     => MulNormal,
            EStarSize.Giant      => MulGiant,
            EStarSize.Supergiant => MulSupergiant,
            _ => MulNormal
        };
    }
}
