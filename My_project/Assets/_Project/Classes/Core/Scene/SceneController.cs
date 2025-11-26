using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Core.Scene
{
    public enum SceneId
    {
        Boot,
        MainMenu,
        GalaxyMap,
        SystemMap
    }

    public class SceneController
    {
        /// Синхронная загрузка сцены (блокирующая).
        public static void Load(SceneId id)
        {
            SceneManager.LoadScene(NameOf(id));
        }
        /// Асинхронная загрузка сцены (через async/await).
        public static async Task LoadAsync(SceneId id)
        {
            var op = SceneManager.LoadSceneAsync(NameOf(id));
            while (op != null && !op.isDone)
            {
                await Task.Yield(); // ждем кадр
            }
        }
        /// Загрузить сцену через заданное время (секунды).
        public static async Task LoadWithDelay(SceneId id, float delaySeconds, bool async = true)
        {
            var safeSeconds = System.Math.Max(0f, delaySeconds);
            var delay = System.TimeSpan.FromSeconds(safeSeconds);
            await Task.Delay(delay);

            if (async)
                await LoadAsync(id);
            else
                Load(id);
        }
        /// Маппинг enum → имя сцены в билде.
        private static string NameOf(SceneId id)
        {
            switch (id)
            {
                case SceneId.Boot: return "Boot";
                case SceneId.MainMenu: return "MainMenu";
                case SceneId.GalaxyMap: return "GalaxyMap";
                case SceneId.SystemMap: return "SystemMap";
                default: return "MainMenu";
            }
        }
    }
}
