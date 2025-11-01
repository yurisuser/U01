using _Project.Scripts.Galaxy.Config;

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
        public static readonly float DefaultPatrolRadius = OrbitMath.PlanetOrbitIndexToUnits(20);
    }
}
