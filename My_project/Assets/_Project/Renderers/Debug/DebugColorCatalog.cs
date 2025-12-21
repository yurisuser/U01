using UnityEngine;

namespace _Project.Scripts.SystemMap.Debug
{
    /// <summary>Яркая палитра для отладочных линий/точек.</summary>
    public static class DebugColorCatalog
    {
        private static readonly Color[] Colors =
        {
            new Color(1f, 0.15f, 0.15f),
            new Color(0.2f, 1f, 0.2f),
            new Color(0.2f, 0.7f, 1f),
            new Color(1f, 1f, 0.2f),
            new Color(1f, 0.2f, 1f),
            new Color(0.2f, 1f, 1f),
            new Color(1f, 0.6f, 0.1f),
            new Color(0.6f, 1f, 0.1f),
            new Color(1f, 0.35f, 0.35f),
            new Color(0.35f, 1f, 0.35f),
            new Color(0.35f, 0.6f, 1f),
            new Color(1f, 0.85f, 0.2f),
        };

        public static Color GetColor(int key)
        {
            if (Colors.Length == 0)
                return Color.white;

            int index = Mathf.Abs(key) % Colors.Length;
            return Colors[index];
        }
    }
}
