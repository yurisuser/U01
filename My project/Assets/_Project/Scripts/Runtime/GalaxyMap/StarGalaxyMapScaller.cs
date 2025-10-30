using UnityEngine;

namespace _Project.Scripts.GalaxyMap.Runtime
{
    public class StarGalaxyMapScaller : MonoBehaviour
    {

        public Camera mainCamera;
        public float baseScale = 0.05f;
        public float scaleFactor = 0.002f;

        void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        void Update()
        {
            float distance = Vector3.Distance(mainCamera.transform.position, transform.position);
            float scale = baseScale + distance * scaleFactor;
            transform.localScale = Vector3.one * scale;
        }
    }

}