using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts
{
    public static class SelectedSystemBus
    {
        public static StarSys Selected;
        public static bool HasValue;
        public static void Clear() { Selected = default; HasValue = false; }
    }
}