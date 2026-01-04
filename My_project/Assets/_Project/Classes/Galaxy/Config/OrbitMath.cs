using _Project.Scripts.Const;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Config
{
    /// <summary>
    /// Вспомогательные функции для перевода орбит в юниты сцены.
    /// </summary>
    public static class OrbitMath
    {
        public static float PlanetOrbitIndexToUnits(int orbitIndex)
        {
            return Mathf.Max(0, orbitIndex) * StarSysemConstants.PlanetOrbitUnit;
        }
    }
}
