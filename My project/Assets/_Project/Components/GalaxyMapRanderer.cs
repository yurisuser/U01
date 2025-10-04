using System.Collections.Generic;
using UnityEngine;
using _Project.Galaxy.Obj; // StarSys, StarType

namespace _Project
{
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("Prefabs by star type")]
        [SerializeField] private GameObject redPrefab;
        [SerializeField] private GameObject orangePrefab;
        [SerializeField] private GameObject yellowPrefab;
        [SerializeField] private GameObject whitePrefab;
        [SerializeField] private GameObject bluePrefab;
        [SerializeField] private GameObject neutronPrefab;
        [SerializeField] private GameObject blackPrefab;
        [SerializeField] private GameObject defaultPrefab;

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
                Debug.LogWarning("GalaxyMapRenderer: нет галактики для отображения.");
            }
        }

        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (systems == null || systems.Length == 0) return;

            if (clearBefore)
            {
                for (int i = 0; i < _spawned.Count; i++)
                    if (_spawned[i] != null) Destroy(_spawned[i]);
                _spawned.Clear();
            }

            _spawned.Capacity = Mathf.Max(_spawned.Capacity, systems.Length);

            for (int i = 0; i < systems.Length; i++)
            {
                var s = systems[i];

                // ВАЖНО: тип — в s.Star.type
                var prefab = GetPrefabFor(s.Star.type);
                if (prefab == null) prefab = defaultPrefab;
                if (prefab == null) continue; // ничего не ставим, если не задано

                var go = Instantiate(prefab, s.GalaxyPosition, Quaternion.identity, starsRoot);
                go.name = string.IsNullOrEmpty(s.Name) ? $"SYS-{i:0000}" : s.Name;
                _spawned.Add(go);
            }
        }

        private GameObject GetPrefabFor(StarType t)
        {
            switch (t)
            {
                case StarType.Red:     return redPrefab;
                case StarType.Orange:  return orangePrefab;
                case StarType.Yellow:  return yellowPrefab;
                case StarType.White:   return whitePrefab;
                case StarType.Blue:    return bluePrefab;
                case StarType.Neutron: return neutronPrefab;
                case StarType.Black:   return blackPrefab;
                default:               return defaultPrefab;
            }
        }
    }
}
