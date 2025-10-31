using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;
using _Project.Prefabs; //  ���������

namespace _Project.Scripts.GalaxyMap.Runtime
{
    [DisallowMultipleComponent]
    public class GalaxyMapRenderer : MonoBehaviour
    {
        [Header("��⠫�� ��䠡�� (�������᪠� ����)")]
        [SerializeField] private PrefabCatalog catalog;          //  ���������

        [Header("��䮫�� ��䠡 (�᫨ � ��⠫��� ��� �祩��)")]
        [SerializeField] private GameObject defaultPrefab;

        [Header("����� �� ࠧ���� (�᫨ �� �㦥� - ���⠢� �� = 1)")]
        [SerializeField] private float dwarfMul      = 0.7f;
        [SerializeField] private float normalMul     = 1.0f;
        [SerializeField] private float giantMul      = 2.4f;
        [SerializeField] private float supergiantMul = 3.8f;
        [SerializeField] private float globalScale   = 4.0f;

        [Header("�㤠 ᪫��뢠�� ���⠭��")]
        [SerializeField] private Transform starsRoot;

        private readonly List<GameObject> _spawned = new();
        private GameStateService _state;

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

        private void OnEnable()
        {
            _state = GameBootstrap.GameState;
            if (_state != null)
            {
                _state.RenderChanged += OnRenderChanged;
                OnRenderChanged(_state.Render); // моментально приводим в синхрон
            }
        }

        private void OnDisable()
        {
            if (_state != null)
                _state.RenderChanged -= OnRenderChanged;
            _state = null;
        }

        private void OnRenderChanged(GameStateService.RenderSnapshot snapshot)
        {
            Render(snapshot.Galaxy, clearBefore: true);
        }

        public void Render(StarSys[] systems, bool clearBefore = true)
        {
            if (clearBefore)
                ClearSpawned();

            if (systems == null || systems.Length == 0) return;

            var parent = starsRoot ? starsRoot : transform;

            for (int i = 0; i < systems.Length; i++)
            {
                var s = systems[i];

                var prefab = GetPrefabFor(s.Star.type) ?? defaultPrefab; //  ���� �� ��⠫���
                if (!prefab) continue;

                var go = Instantiate(prefab, s.GalaxyPosition, Quaternion.identity, parent);
                go.name = string.IsNullOrWhiteSpace(s.Name) ? $"SYS-{i:0000}" : s.Name;

                // ����⠡ �� ࠧ���� �� ������ �����樨
                var mul = GetSizeMul(s.Star.size) * Mathf.Max(0.0001f, globalScale);
                go.transform.localScale = go.transform.localScale * mul;

                // �᫨ �� ��䠡� ���� ������ - �ப��뢠�� �����
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

        private void ClearSpawned()
        {
            for (int i = 0; i < _spawned.Count; i++)
            {
                var go = _spawned[i];
                if (!go) continue;
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(go);
                else
#endif
                    Destroy(go);
            }
            _spawned.Clear();
        }

        // === �������쭠� �ࠢ��: �⠥� ��䠡 �� PrefabCatalog ===
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
