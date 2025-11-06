using System.Collections.Generic;
using _Project.Prefabs;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.Simulation.PilotMotivation;
using UnityEngine;

namespace _Project.Scripts.SystemMap
{
    [DisallowMultipleComponent]
    public sealed class SystemMapShipRenderer : MonoBehaviour, ISystemMapLayer
    {
        [Header("Порядок слоя")]
        [SerializeField] private int order = 10;
        public int Order => order;

        [Header("Каталог префабов")]
        [SerializeField] private PrefabCatalog catalog;

        private Transform _root;
        private readonly Dictionary<UID, GameObject> _views = new();
        private readonly Dictionary<UID, Ship> _prevShips = new();
        private readonly Dictionary<UID, LineRenderer> _paths = new();

        public void Init(Transform parentRoot)
        {
            if (_root == null)
            {
                var go = new GameObject("ShipsRoot");
                go.transform.SetParent(parentRoot, false);
                _root = go.transform;
            }
        }

        public void Render(in StarSys sys,
                            Ship[] prevShips,
                            int prevCount,
                            Ship[] currShips,
                            int currCount,
                            Ship[] nextShips,
                            int nextCount,
                            float progress,
                            float stepDuration)
        {
            progress = Mathf.Clamp01(progress);
            stepDuration = Mathf.Max(0.0001f, stepDuration);
            var runtimeContext = RuntimeWorldService.Instance?.Context;
            var pilots = runtimeContext?.Pilots;

            _prevShips.Clear();
            if (prevShips != null && prevCount > 0)
            {
                for (int i = 0; i < prevCount; i++)
                    _prevShips[prevShips[i].Uid] = prevShips[i];
            }

            var seen = HashSetPool<UID>.Get();

            if (currShips != null && currCount > 0)
            {
                for (int i = 0; i < currCount; i++)
                {
                    var sh = currShips[i];
                    seen.Add(sh.Uid);

                    if (!_views.TryGetValue(sh.Uid, out var view) || !view)
                    {
                        var prefab = GetShipPrefab();
                        if (!prefab)
                            continue;

                        view = Instantiate(prefab, _root);
                        view.name = $"Ship_{sh.Uid.Id}";
                        _views[sh.Uid] = view;
                    }

                    Vector3 startPos = sh.Position;
                    Quaternion startRot = sh.Rotation;
                    Vector3 startVel = sh.Velocity;
                    if (_prevShips.TryGetValue(sh.Uid, out var prev))
                    {
                        startPos = prev.Position;
                        startRot = prev.Rotation;
                        startVel = prev.Velocity;
                    }

                    var pos = InterpolatePosition(startPos, sh.Position, startVel, sh.Velocity, stepDuration, progress);
                    var rot = Quaternion.Slerp(startRot, sh.Rotation, progress);

                    view.transform.localPosition = pos;
                    view.transform.localRotation = rot;

                    Vector3 target;
                    bool hasTarget = TryGetDestination(pilots, sh.PilotUid, out target);
                    UpdatePathRenderer(sh.Uid, pos, target, hasTarget);
                }
            }

            if (_views.Count > 0)
            {
                ScratchKeys.Clear();
                ScratchKeys.AddRange(_views.Keys);

                for (int k = 0; k < ScratchKeys.Count; k++)
                {
                    var id = ScratchKeys[k];
                    if (!seen.Contains(id))
                    {
                        if (_paths.TryGetValue(id, out var path) && path)
                            Destroy(path.gameObject);
                        _paths.Remove(id);

                        if (_views[id])
                            Destroy(_views[id]);
                        _views.Remove(id);
                    }
                }
            }

            HashSetPool<UID>.Release(seen);
            _prevShips.Clear();
        }

        public void Dispose() => ClearAll();

        private GameObject GetShipPrefab()
        {
            if (!catalog || catalog.ShipPrefabsByClass == null || catalog.ShipPrefabsByClass.Length == 0)
                return null;

            return catalog.ShipPrefabsByClass[0];
        }

        private void ClearAll()
        {
            foreach (var kv in _views)
            {
                if (kv.Value)
                    Destroy(kv.Value);
            }
            _views.Clear();

            foreach (var path in _paths.Values)
            {
                if (path)
                    Destroy(path.gameObject);
            }
            _paths.Clear();

            if (_root)
            {
                for (int i = _root.childCount - 1; i >= 0; i--)
                    Destroy(_root.GetChild(i).gameObject);
            }

            _prevShips.Clear();
        }

        private static class HashSetPool<T>
        {
            private static readonly Stack<HashSet<T>> Pool = new();

            public static HashSet<T> Get() => Pool.Count > 0 ? Pool.Pop() : new HashSet<T>();

            public static void Release(HashSet<T> set)
            {
                set.Clear();
                Pool.Push(set);
            }
        }

        private static readonly List<UID> ScratchKeys = new();

        private static Vector3 InterpolatePosition(Vector3 p0, Vector3 p1, Vector3 v0, Vector3 v1, float duration, float t)
        {
            t = Mathf.Clamp01(t);
            if (duration <= 0f)
                return Vector3.Lerp(p0, p1, t);

            var delta = p1 - p0;
            float deltaSqr = delta.sqrMagnitude;
            if (deltaSqr < 1e-8f)
                return p0;

            var dir = delta / Mathf.Sqrt(deltaSqr);

            float v0Along = Vector3.Dot(v0, dir);
            if (v0Along < 0f)
                v0 -= dir * v0Along;

            float v1Along = Vector3.Dot(v1, dir);
            if (v1Along < 0f)
                v1 -= dir * v1Along;

            var m0 = v0 * duration;
            var m1 = v1 * duration;

            float t2 = t * t;
            float t3 = t2 * t;

            float h00 = 2f * t3 - 3f * t2 + 1f;
            float h10 = t3 - 2f * t2 + t;
            float h01 = -2f * t3 + 3f * t2;
            float h11 = t3 - t2;

            var result = h00 * p0 + h10 * m0 + h01 * p1 + h11 * m1;

            // fallback guard in case Hermite overshoots massively (e.g. zero previous state)
            if (float.IsNaN(result.x) || float.IsNaN(result.y) || float.IsNaN(result.z) ||
                float.IsInfinity(result.x) || float.IsInfinity(result.y) || float.IsInfinity(result.z))
                return Vector3.Lerp(p0, p1, t);

            return result;
        }

        private static bool TryGetDestination(PilotRegistry pilots, UID pilotUid, out Vector3 destination)
        {
            destination = Vector3.zero;
            if (pilots == null)
                return false;

            if (!pilots.TryGetMotiv(pilotUid, out var motive))
                return false;

            if (motive.TryPeekAction(out var action) && action.Action == EAction.MoveToCoordinates)
            {
                destination = action.Parameters.Move.Destination;
                return true;
            }

            return false;
        }

        private void UpdatePathRenderer(UID uid, in Vector3 startPos, in Vector3 targetPos, bool hasTarget)
        {
            if (!_paths.TryGetValue(uid, out var line) || !line)
            {
                if (!hasTarget)
                    return;

                line = CreatePathRenderer();
                _paths[uid] = line;
            }

            if (!hasTarget)
            {
                line.gameObject.SetActive(false);
                return;
            }

            line.gameObject.SetActive(true);
            line.positionCount = 2;
            line.SetPosition(0, startPos);
            line.SetPosition(1, targetPos);
        }

        private LineRenderer CreatePathRenderer()
        {
            var go = new GameObject("ShipPath");
            go.transform.SetParent(_root, false);
            var line = go.AddComponent<LineRenderer>();
            line.useWorldSpace = false;
            line.material = GetPathMaterial();
            line.widthMultiplier = 0.05f;
            line.positionCount = 0;
            line.startColor = new Color(0.3f, 0.8f, 1f, 0.6f);
            line.endColor = new Color(0.3f, 0.8f, 1f, 0.2f);
            return line;
        }

        private static Material GetPathMaterial()
        {
            if (_pathMaterial == null)
            {
                var shader = Shader.Find("Sprites/Default");
                _pathMaterial = shader != null
                    ? new Material(shader)
                    : new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended"));
                _pathMaterial.hideFlags = HideFlags.HideAndDontSave;
            }

            return _pathMaterial;
        }

        private static Material _pathMaterial;
    }
}
