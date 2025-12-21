using UnityEngine;

namespace _Project.Scripts.SystemMap.Debug
{
    /// <summary>Яркая палитра для отладочных линий/точек.</summary>
    public static class DebugColorCatalog
    {
        private const float GoldenRatioConjugate = 0.6180339887f;

        public static Color GetColor(int key)
        {
            float hue = Mathf.Repeat(Mathf.Abs(key) * GoldenRatioConjugate, 1f);
            return Color.HSVToRGB(hue, 0.9f, 0.9f);
        }
    }
}
