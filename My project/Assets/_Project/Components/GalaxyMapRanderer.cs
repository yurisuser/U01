using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Galaxy.Obj; // StarSys

namespace _Project
{
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("Prefab")]
        [SerializeField] private GameObject starPrefab;
        [SerializeField] private Transform starsRoot;
        private readonly List<GameObject> _spawned = new();
        
        private void Awake()
        {
            if (starsRoot == null)
            {
                var go = new GameObject("StarsRoot");
                go.transform.position = Vector3.zero;
                starsRoot = go.transform;
            }
        }
        private void Start()
        {
            var galaxy = Core.Galaxy;
            if (galaxy != null && galaxy.Length > 0)
            {
                Render(galaxy);
            }
            else
            {
                Debug.LogWarning("GalaxyDrawer: нет галактики для отображения.");
            }
        }

        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (starPrefab == null)
            {
                Debug.LogWarning($"{nameof(GalaxyMapRenderer)}: назначь Star Prefab в инспекторе.");
                return;
            }
            _spawned.Capacity = Mathf.Max(_spawned.Capacity, systems.Length);
            for (int i = 0; i < systems.Length; i++)
            {
                var go = Instantiate(starPrefab, systems[i].GalaxyPosition, Quaternion.identity, starsRoot);
                go.name = string.IsNullOrEmpty(systems[i].Name) ? $"SYS-{i:0000}" : systems[i].Name;
                _spawned.Add(go);
            }
        }
    }
}
