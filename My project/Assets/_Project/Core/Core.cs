using UnityEngine;
using _Project.Inputs;
namespace _Project
{
    public class Core : MonoBehaviour
    {
        public static Core Instance { get; private set; }

       public InputController Input { get; private set; }

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else if (Instance != this) { Destroy(gameObject); return; }
            Input = new InputController();  
        }

        private void Update()
        {
            Input?.Update();  
        }
    }
}