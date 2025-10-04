﻿namespace _Project.CONSTANT
{
    public static class GALAXY {
        // Орбиты
        public const int OrbitSlots  = 20; // количество орбит вокруг звезды/планеты
        // Типы звёзд (вес)
        public const int StarRedWeight     = 10; // красные
        public const int StarOrangeWeight  = 10; // оранжевые
        public const int StarYelloWeight   = 10;  // жёлтые
        public const int StarWhiteWeight   = 7;  // белые
        public const int StarBlueWeight    = 5;  // синие
        public const int StarNeutronWeight = 3;  // нейтронные
        public const int StarBlackWeight   = 3;  // чёрные дыры
        // Размеры звёзд Red
        public const int RedDwarfWeight      = 70; // карлик
        public const int RedNormalWeight     = 25; // обычная
        public const int RedGiantWeight      = 5;  // гигант
        public const int RedSuperGiantWeight = 0;  // сверхгигант
        // Размеры звёзд Solar (Orange/Yellow/White)
        public const int SolarDwarfWeight      = 20; // карлик
        public const int SolarNormalWeight     = 60; // обычная
        public const int SolarGiantWeight      = 20; // гигант
        public const int SolarSuperGiantWeight = 0;  // сверхгигант
        // Размеры звёзд Blue
        public const int BlueDwarfWeight      = 0;  // карлик (редкость)
        public const int BlueNormalWeight     = 10; // обычная
        public const int BlueGiantWeight      = 60; // гигант
        public const int BlueSuperGiantWeight = 30; // сверхгигант
        // Температуры (K)
        public static readonly (float min, float max) TempRed     = (2600f, 4000f);   // красные
        public static readonly (float min, float max) TempOrange  = (3900f, 5200f);   // оранжевые
        public static readonly (float min, float max) TempYello   = (5200f, 6000f);   // жёлтые
        public static readonly (float min, float max) TempWhite   = (6000f, 9000f);   // белые
        public static readonly (float min, float max) TempBlue    = (10000f, 30000f); // синие
        public static readonly (float min, float max) TempNeutron = (1.0e5f, 1.0e6f); // нейтронные
        public static readonly (float min, float max) TempBlack   = (0f, 10f);        // чёрные дыры
        // Профили звёзд (масса, радиус, светимость)
        public static readonly (float mMin, float mMax, float rMin, float rMax, float lMin, float lMax) ProfDwarf
            = (0.08f, 0.8f, 0.1f, 0.9f, 0.0001f, 0.1f); // красные карлики
        public static readonly (float mMin, float mMax, float rMin, float rMax, float lMin, float lMax) ProfNormal
            = (0.8f, 1.5f, 0.8f, 1.3f, 0.5f, 5f);       // солнцеподобные
        public static readonly (float mMin, float mMax, float rMin, float rMax, float lMin, float lMax) ProfGiant
            = (1.0f, 8.0f, 10f, 100f, 100f, 10000f);    // гиганты
        public static readonly (float mMin, float mMax, float rMin, float rMax, float lMin, float lMax) ProfSuper
            = (8.0f, 40f, 100f, 1000f, 1.0e4f, 1.0e6f); // сверхгиганты
        // Типы планет (вес)
        public const int PlanetRockyWeight = 40; // каменные (землеподобные)
        public const int PlanetGasGiantWeight    = 20; // газовые гиганты
        public const int PlanetIceGiantWeight    = 10; // ледяные гиганты
        public const int PlanetDwarfWeight       = 10; // карликовые
        public const int PlanetOceanWeight       = 5;  // океанические
        public const int PlanetDesertWeight      = 5;  // пустынные
        public const int PlanetLavaWeight        = 5;  // вулканические
        public const int PlanetFrozenWeight      = 3;  // замёрзшие
        public const int PlanetToxicWeight       = 2;  // токсичные
        // Типы лун (вес)
        public const int MoonRockyWeight    = 40; // каменистые
        public const int MoonIcyWeight      = 25; // ледяные
        public const int MoonVolcanicWeight = 10; // вулканические
        public const int MoonDesertWeight   = 10; // пустынные
        public const int MoonOceanWeight    = 8;  // океанические
        public const int MoonCapturedWeight = 7;  // захваченные астероиды
        // Профили планет (масса, радиус)
        public static readonly (float mMin, float mMax, float rMin, float rMax) PlanetSmall
            = (0.01f, 0.5f, 0.2f, 0.7f);  // марсоподобные
        public static readonly (float mMin, float mMax, float rMin, float rMax) PlanetMedium
            = (0.5f, 5f, 0.7f, 1.2f);     // землеподобные
        public static readonly (float mMin, float mMax, float rMin, float rMax) PlanetLarge
            = (5f, 50f, 1.5f, 4f);        // урано-нептуноподобные
        public static readonly (float mMin, float mMax, float rMin, float rMax) PlanetHuge
            = (50f, 300f, 4f, 15f);       // юпитероидные
        // Профили лун (масса, радиус)
        public static readonly (float mMin, float mMax, float rMin, float rMax) MoonTiny
            = (1e-6f, 5e-4f, 0.01f, 0.06f); // ~100 км
        public static readonly (float mMin, float mMax, float rMin, float rMax) MoonSmall
            = (5e-4f, 1e-2f, 0.06f, 0.25f); // ~100–1000 км
        public static readonly (float mMin, float mMax, float rMin, float rMax) MoonMedium
            = (1e-2f, 0.2f, 0.25f, 0.5f);   // ~1000–3000 км
        public static readonly (float mMin, float mMax, float rMin, float rMax) MoonLarge
            = (0.2f, 1.0f, 0.5f, 0.9f);     // >3000 км
        //вероятность планеты на орбите в зависимости от типа звезды
        public static readonly int[] RedStarPlanetSpawn =
            { 70, 65, 50, 35, 25, 18, 12,  9,  7,  5, 4, 3, 2, 2, 1, 1, 1, 1, 1, 1 };
        public static readonly int[] OrangeStarPlanetSpawn =
            { 60, 70, 65, 55, 45, 35, 25, 18, 12,  9, 7, 5, 4, 3, 2, 1, 1, 1, 1, 1 };
        public static readonly int[] YellowStarPlanetSpawn =
            { 55, 70, 75, 70, 60, 50, 40, 30, 22, 16,12, 9, 7, 5, 4, 3, 2, 2, 2, 2 };
        public static readonly int[] WhiteStarPlanetSpawn =
            { 25, 35, 50, 65, 75, 70, 60, 50, 40, 30,22,16,12, 9, 7, 5, 4, 3, 2, 1 };
        public static readonly int[] BlueStarPlanetSpawn =
            {  8, 12, 18, 25, 35, 50, 65, 75, 80, 75,65,55,45,35,25,18,12, 9, 7, 5 };
        public static readonly int[] NeutronStarPlanetSpawn =
            {  2,  2,  3,  3,  3,  3,  3,  3,  2,  2, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 };
        public static readonly int[] BlackStarPlanetSpawn =
            {  1,  1,  2,  2,  2,  2,  2,  2,  1,  1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    }
}
