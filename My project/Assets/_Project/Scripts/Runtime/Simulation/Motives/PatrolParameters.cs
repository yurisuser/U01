namespace _Project.Scripts.Simulation.Motives
{
    /// <summary>
    /// Общие параметры поведения патруля.
    /// </summary>
    public static class PatrolParameters
    {
        /// <summary>
        /// Радиус патруля по умолчанию (20-я орбита) в единицах сцены.
        /// </summary>
        public const float DefaultPatrolRadius = 200f; // = OrbitMath.PlanetOrbitIndexToUnits(20)
    }
}
