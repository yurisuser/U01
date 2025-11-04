using UnityEngine;

namespace _Project.Scripts.Galaxy.Config
{
    /// <summary>
    /// Вспомогательные функции для перевода орбит в юниты сцены.
    /// </summary>
    public static class OrbitMath
    {
        private const float PlanetOrbitUnit = 10f; // синхронизировано с PlanetSysCreator

        public static float PlanetOrbitIndexToUnits(int orbitIndex)
        {
            return Mathf.Max(0, orbitIndex) * PlanetOrbitUnit;
        }
    }
}
