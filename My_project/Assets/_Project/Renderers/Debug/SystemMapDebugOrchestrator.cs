using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.DebugState;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Ships;
using UnityEngine;

namespace _Project.Scripts.SystemMap.Debug
{
    /// <summary>Оркестратор отладочного рендера карты системы.</summary>
    [DisallowMultipleComponent]
    public sealed class SystemMapDebugOrchestrator : MonoBehaviour
    {
        [Header("Включение слоёв")]
        public bool showPaths = true;
        public bool showPoints = true;

        [Header("Параметры путей")]
        [SerializeField] private int maxPathPoints = 300;

        [Header("Подчинённые рендереры")]
        [SerializeField] private DebugPathsRenderer pathsRenderer;
        [SerializeField] private DebugPointsRenderer pointsRenderer;
        [SerializeField] private Transform drawRoot;

        private readonly DebugPathsState _pathsState = new DebugPathsState();
        private readonly DebugPointsState _pointsState = new DebugPointsState();
        private readonly HashSet<int> _activeIds = new HashSet<int>();

        private void Awake()
        {
            if (!pathsRenderer)
                pathsRenderer = GetComponent<DebugPathsRenderer>() ?? gameObject.AddComponent<DebugPathsRenderer>();
            if (!pointsRenderer)
                pointsRenderer = GetComponent<DebugPointsRenderer>() ?? gameObject.AddComponent<DebugPointsRenderer>();

            pathsRenderer.Bind(_pathsState);
            pointsRenderer.Bind(_pointsState);
            if (!drawRoot)
                drawRoot = transform;
            pathsRenderer.SetRoot(drawRoot);
            pointsRenderer.SetRoot(drawRoot);
        }

        public void SetRoot(Transform root)
        {
            drawRoot = root ? root : transform;
            if (pathsRenderer)
                pathsRenderer.SetRoot(drawRoot);
            if (pointsRenderer)
                pointsRenderer.SetRoot(drawRoot);
        }

        private void Update()
        {
            if (pathsRenderer)
                pathsRenderer.enabled = showPaths;
            if (pointsRenderer)
                pointsRenderer.enabled = showPoints;

            if (!showPaths && !showPoints)
                return;

            var gameState = GameBootstrap.GameState;
            var system = gameState.GetSelectedSystem();
            if (system == null || system.Value.State == null)
            {
                if (showPoints)
                    _pointsState.Clear();
                if (showPaths)
                    _pathsState.Clear();
                return;
            }

            var ships = system.Value.State.CurrShipSnapshots;
            if (showPoints)
                _pointsState.Clear();
            _activeIds.Clear();

            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                int uid = ship.Uid.Id;
                _activeIds.Add(uid);
                var color = DebugColorCatalog.GetColor(uid);

                if (showPaths)
                    _pathsState.AddPoint(uid, ship.Position, color, maxPathPoints);

                if (showPoints && TryResolveTarget(in ship, out var target))
                    _pointsState.AddPoint(uid, target, color);
            }

            if (showPaths)
                _pathsState.RemoveMissing(_activeIds);
        }

        private static bool TryResolveTarget(in Ship ship, out Vector3 target)
        {
            if (ship.TaskState.TryPeek(out var task) && task.Type == ShipTaskType.MoveToPoint)
            {
                target = task.Params.MoveToPointParams.Destination;
                return true;
            }

            target = default;
            return false;
        }
    }
}
