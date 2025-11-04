using UnityEngine;
using _Project.Scripts.Core.Runtime;

namespace _Project.Scripts.Core
{
    /// <summary>
    /// Singleton that owns the runtime context and survives scene changes.
    /// </summary>
    public sealed class RuntimeWorldService : MonoBehaviour
    {
        private static RuntimeWorldService _instance;

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

        public static RuntimeContext RequireContext()
        {
            return Instance.Context ??= new RuntimeContext();
        }

        public void ResetContext()
        {
            Context ??= new RuntimeContext();
            Context.Reset();
        }
    }
}
