using UnityEngine;
using _Project.Scripts.Core.Runtime;

namespace _Project.Scripts.Core
{
    /// <summary>Singleton, владеющий RuntimeContext и живущий между сценами.</summary>
    public sealed class RuntimeWorldService : MonoBehaviour
    {
        private static RuntimeWorldService _instance;

        /// <summary>Единственный экземпляр сервиса.</summary>
        public static RuntimeWorldService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject(nameof(RuntimeWorldService));
                    _instance = go.AddComponent<RuntimeWorldService>();
                }

                return _instance;
            }
        }

        /// <summary>Текущий RuntimeContext.</summary>
        public RuntimeContext Context { get; private set; }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            Context ??= new RuntimeContext();
        }

        /// <summary>Гарантированно возвращает контекст, создавая при отсутствии.</summary>
        public static RuntimeContext RequireContext()
        {
            return Instance.Context ??= new RuntimeContext();
        }

        /// <summary>Полностью сбрасывает/создает новый контекст.</summary>
        public void ResetContext()
        {
            Context ??= new RuntimeContext();
            Context.Reset();
        }
    }
}
