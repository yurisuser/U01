namespace _Project.Scripts.Const
{
    /// <summary>Константы для звездных систем (системная карта, орбиты, масштабы префабов).</summary>
    public static class StarSysemConstants
    {
        // Перевод «индекса орбиты» планеты в сценовые юниты.
        // Если у тебя уже есть своя функция масштаба — просто подставь её.
        // Юнитов сцены на 1 «клетку» орбиты планеты.
        public const float PlanetOrbitUnit = 100f;

        // Перевод «индекса орбиты» луны в сценовые юниты относительно ПЛАНЕТЫ.
        // Радиус внешней лунной орбиты = lastMoonIndex * этот множитель.
        public const float MoonOrbitUnit = 8f;

        // Перевод радиуса планеты (в земных радиусах) в сценовые юниты её визуала/габарита.
        public const float PlanetRadiusToUnits = 0.6f;

        // Масштаб префабов по радиусу: реальный радиус * PrefabScale.
        public const float StarPrefabScale = 12f;
        public const float PlanetPrefabScale = 1f;
        public const float MoonPrefabScale = 1f;

        // Базовые масштабы орбит на системной карте.
        public const float PlanetOrbitScale = 1f;
        public const float MoonOrbitScale = 1f;

        // Ограничение визуального радиуса звезды на карте системы (доля первой орбиты).
        public const float StarMaxRadiusOrbitFactor = 0.75f;

        // Базовая минимальная угловая дистанция (радианы) «против прилипания»,
        // добавляется к расчётной ширине от габаритов систем.
        public const float MinAngularGapBase = 0.10f; // ≈ 5.7°

        // Шаг подбора угла (золотой угол, чтобы не «резонировать» с кратными делениями круга).
        public const float GoldenAngleRad = 2.39996322972865332f; // 360° * (φ - 1)^2

        // Сколько попыток смещения угла делаем, если рядом уже занято.
        public const int MaxAngleAttempts = 64;

        // Радиус внутренней «мертвой зоны» в орбитах.
        public const float InnerDeadZoneOrbits = 1f;

        // Слоты орбит вокруг звезды/планеты.
        public const int OrbitSlots = 20;
    }
}
