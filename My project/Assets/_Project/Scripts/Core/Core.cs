using System.Collections;
using _Project.Scripts.Core.Input;
using _Project.Scripts.Core.Scene;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Galaxy.Generation;
using UnityEngine;

namespace _Project.Scripts.Core
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
            UnityEngine.Debug.Log($"Core: Galaxy generated. Stars = {Galaxy}");
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