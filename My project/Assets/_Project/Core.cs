using UnityEngine;
using System.Collections;
using _Project.Galaxy;
using _Project.Galaxy.Obj;

namespace _Project
{
    public sealed class Core : MonoBehaviour
    {
        public static Core Instance { get; private set; }
        public SceneController Scenes { get; } = new SceneController();
        public  static StarSys[] Galaxy { get; private set; }
        public InputController  Input  { get; } = new InputController();
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            //
            Galaxy = GalaxyCreator.Create();
            Debug.Log($"Core: Galaxy generated. Stars = {Galaxy}");
            //
            StartCoroutine(LoadMainMenuDelayed());///////////////////////////////////////
        }
        private void Update()
        {
            Input?.Update(); 
        }
        
        private IEnumerator LoadMainMenuDelayed()
        {
            yield return new WaitForSeconds(1f);
            SceneController.Load(SceneId.GalaxyMap); // статический вызов
        }
    }
}