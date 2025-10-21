using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using _Project.Prefabs; // ← добавлено

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("Каталог префабов (галактическая карта)")]
        [SerializeField] private PrefabCatalog catalog;          // ← добавлено

        [Header("Дефолтный префаб (если в каталоге нет ячейки)")]
        [SerializeField] private GameObject defaultPrefab;

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
            var galaxy = Core.Core.Galaxy; // StarSys[]
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

                var prefab = GetPrefabFor(s.Star.type) ?? defaultPrefab; // ← берём из каталога
                if (!prefab) continue;

                var go = Instantiate(prefab, s.GalaxyPosition, Quaternion.identity, starsRoot);
                go.name = string.IsNullOrWhiteSpace(s.Name) ? $"SYS-{i:0000}" : s.Name;

                // масштаб по размеру из данных генерации
                var mul = GetSizeMul(s.Star.size) * Mathf.Max(0.0001f, globalScale);
                go.transform.localScale = go.transform.localScale * mul;

                // если на префабе есть кликер — прокидываем данные
                var click = go.GetComponent<StarGalaxyMapClick>();
                if (click != null)
                {
                    click.type       = s.Star.type;
                    click.systemName = go.name;
                    click.System     = s;
                }

                _spawned.Add(go);
            }
        }

        // === минимальная правка: читаем префаб из PrefabCatalog ===
        private GameObject GetPrefabFor(EStarType t)
        {
            if (!catalog || catalog.StarGalaxyPrefabsByType == null) return null;
            int idx = (int)t;
            var arr = catalog.StarGalaxyPrefabsByType;
            if (idx < 0 || idx >= arr.Length) return null;
            return arr[idx];
        }
        // ==========================================================

        private float GetSizeMul(EStarSize z) =>
            z switch
            {
                EStarSize.Dwarf      => dwarfMul,
                EStarSize.Normal     => normalMul,
                EStarSize.Giant      => giantMul,
                EStarSize.Supergiant => supergiantMul,
                _                    => normalMul
            };
    }
}
