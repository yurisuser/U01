using System.Collections.Generic;
using UnityEngine;
using _Project.Galaxy.Obj; // StarSys, StarType, StarSize

namespace _Project.Components
{
    [DisallowMultipleComponent]
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("Префабы по типам звёзд")]
        [SerializeField] private GameObject redPrefab;
        [SerializeField] private GameObject orangePrefab;
        [SerializeField] private GameObject yellowPrefab;
        [SerializeField] private GameObject whitePrefab;
        [SerializeField] private GameObject bluePrefab;
        [SerializeField] private GameObject neutronPrefab;
        [SerializeField] private GameObject blackPrefab;
        [SerializeField] private GameObject defaultPrefab; // можно оставить пустым

        [Header("Скейл по размеру (если не нужен — поставь все = 1)")]
        [SerializeField] private float dwarfMul      = 0.7f;
        [SerializeField] private float normalMul     = 1.0f;
        [SerializeField] private float giantMul      = 2.4f;
        [SerializeField] private float supergiantMul = 3.8f;
        [SerializeField] private float globalScale   = 4.0f;

        [Header("Куда складывать инстансы")]
        [SerializeField] private Transform starsRoot;

        private readonly List<GameObject> _spawned = new();
        public IReadOnlyList<GameObject> Spawned => _spawned;

        private void Awake()
        {
            if (!starsRoot)
            {
                var root = new GameObject("StarsRoot");
                root.transform.SetParent(transform, false);
                starsRoot = root.transform;
            }
        }

        private void Start()
        {
            var galaxy = Core.Galaxy; // StarSys[]
            if (galaxy != null && galaxy.Length > 0)
                Render(galaxy, clearBefore: true);
        }

        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (systems == null || systems.Length == 0) return;

            if (clearBefore)
            {
                for (int i = 0; i < _spawned.Count; i++)
                    if (_spawned[i]) Destroy(_spawned[i]);
                _spawned.Clear();
            }

            for (int i = 0; i < systems.Length; i++)
            {
                var s = systems[i];

                var prefab = GetPrefabFor(s.Star.type) ?? defaultPrefab;
                if (!prefab) continue; // слот не задан — пропускаем

                var go = Instantiate(prefab, s.GalaxyPosition, Quaternion.identity, starsRoot);
                go.name = string.IsNullOrWhiteSpace(s.Name) ? $"SYS-{i:0000}" : s.Name;

                // масштаб по размеру из данных генерации
                var mul = GetSizeMul(s.Star.size) * Mathf.Max(0.0001f, globalScale);
                go.transform.localScale = go.transform.localScale * mul;

                _spawned.Add(go);
            }
        }

        private GameObject GetPrefabFor(StarType t) =>
            t switch
            {
                StarType.Red     => redPrefab,
                StarType.Orange  => orangePrefab,
                StarType.Yellow  => yellowPrefab,
                StarType.White   => whitePrefab,
                StarType.Blue    => bluePrefab,
                StarType.Neutron => neutronPrefab,
                StarType.Black   => blackPrefab,
                _                => defaultPrefab
            };

        private float GetSizeMul(StarSize z) =>
            z switch
            {
                StarSize.Dwarf      => dwarfMul,
                StarSize.Normal     => normalMul,
                StarSize.Giant      => giantMul,
                StarSize.Supergiant => supergiantMul,
                _                   => normalMul
            };
    }
}
