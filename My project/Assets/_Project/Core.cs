using UnityEngine;
using System.Collections;

namespace _Project
{
    public sealed class Core : MonoBehaviour
    {
        public static Core Instance { get; private set; }
        public SceneController Scenes { get; } = new SceneController();
        public InputController  Input  { get; } = new InputController();
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(LoadMainMenuDelayed());///////////////////////////////////////
        }
        private void Update()
        {
            Input?.Update(); 
        }
        
        private IEnumerator LoadMainMenuDelayed()
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.MainMenu); // статический вызов
        }
    }
}