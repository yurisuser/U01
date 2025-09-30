using System.Threading.Tasks;
using UnityEngine.SceneManagement;

namespace _Project
{
    public enum SceneId
    {
        Boot,
        MainMenu,
        Level01,
        Level02
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
            int ms = (int)(delaySeconds * 1000);
            await Task.Delay(ms);

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
                case SceneId.Level01: return "Level_01";
                case SceneId.Level02: return "Level_02";
                default: return "MainMenu";
            }
        }
    }
}