using System.Collections.Generic;
using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation;
using _Project.Scripts.Simulation.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Continuum
{
    /// <summary>Сервис Continuum: хранит зоны и активные транзиты, тикается в глобальной симуляции.</summary>
    public sealed class ContinuumService
    {
        public static ContinuumService Instance { get; private set; }
        private readonly object _sync = new object(); // Единый lock для транзитов и зон континуума.

        private readonly List<ContinuumTransit> _transits = new(32); // Активные прыжки
        private readonly Dictionary<int, List<ContinuumZone>> _zonesBySystem = new(); // Зоны на систему
        private int _cachedGalaxyVersion = -1; // Кэш для пересчёта зон
        private int _cachedHyperEdgeCount = -1;

        public IReadOnlyList<ContinuumTransit> Transits => _transits;
        public IReadOnlyDictionary<int, List<ContinuumZone>> ZonesBySystem => _zonesBySystem;

        public ContinuumService()
        {
            Instance ??= this;
        }

        /// <summary>Тик глобальной симуляции: уменьшает таймеры и шлёт события прибытия.</summary>
        public void Tick(in SimulationStepContext context)
        {
            if (context.IsPaused || context.GameState == null)
                return;

            EnsureZones(context.GameState);

            lock (_sync)
            {
                var eventBus = context.EventBus;
                for (int i = _transits.Count - 1; i >= 0; i--)
                {
                    var transit = _transits[i];
                    transit.RemainingTurns -= 1;

                    if (transit.RemainingTurns <= 0)
                    {
                        TryPlaceArrivedShip(in transit, in context);
                        _transits.RemoveAt(i);
                        continue;
                    }

                    _transits[i] = transit;
                }
            }
        }

        /// <summary>Поставить прыжок в очередь.</summary>
        public void Enqueue(in ContinuumTransit transit)
        {
            lock (_sync)
            {
                _transits.Add(transit);
            }
        }

        /// <summary>Создать транзит и выставить кораблю ориентацию/скорость по линии прыжка.</summary>
        public ContinuumTransit CreateTransit(Ship ship, int fromSystemIndex, int toSystemIndex, StarSys[] galaxy)
        {
            if (ship != null && galaxy != null &&
                IsValidSystemIndex(fromSystemIndex, galaxy) &&
                IsValidSystemIndex(toSystemIndex, galaxy))
            {
                var dir = (galaxy[toSystemIndex].GalaxyPosition - galaxy[fromSystemIndex].GalaxyPosition).normalized;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    ship.Rotation = Quaternion.LookRotation(dir);
                    ship.CurrentSpeed = ShipSpeed.GetWarpSpeed(in ship);
                }
            }

            return ContinuumTransit.Create(
                ship,
                fromSystemIndex,
                toSystemIndex,
                ContinuumConsts.JumpDurationTurns);
        }

        public bool TryGetZone(int fromSystemIndex, int toSystemIndex, out ContinuumZone zone)
        {
            zone = default;
            lock (_sync)
            {
                if (_zonesBySystem.TryGetValue(fromSystemIndex, out var list))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        if (list[i].TargetSystemIndex == toSystemIndex)
                        {
                            zone = list[i];
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>Пересчитать зоны, если изменилась галактика или список гиперлинков.</summary>
        public void EnsureZones(GameStateService gameState)
        {
            var galaxy = gameState?.Galaxy;
            var edges = gameState?.HyperlinkEdges;

            if (galaxy == null || galaxy.Length == 0 || edges == null)
                return;

            lock (_sync)
            {
                int version = galaxy.Length;
                int edgesCount = edges.Length;
                if (version == _cachedGalaxyVersion && edgesCount == _cachedHyperEdgeCount)
                    return;

                RebuildZones(galaxy, edges);
                _cachedGalaxyVersion = version;
                _cachedHyperEdgeCount = edgesCount;
            }
        }

        private void RebuildZones(StarSys[] galaxy, HyperlinkEdge[] edges)
        {
            _zonesBySystem.Clear();

            for (int i = 0; i < edges.Length; i++)
            {
                var edge = edges[i];
                if (!IsValidSystemIndex(edge.A, galaxy) || !IsValidSystemIndex(edge.B, galaxy))
                    continue;

                var sysA = galaxy[edge.A];
                var sysB = galaxy[edge.B];

                Vector3 dirAB = (sysB.GalaxyPosition - sysA.GalaxyPosition).normalized;
                Vector3 dirBA = -dirAB;

                float radiusA = ComputeZoneRadius(sysA);
                float radiusB = ComputeZoneRadius(sysB);

                float offsetA = ComputeZoneOffset(sysA);
                float offsetB = ComputeZoneOffset(sysB);

                var zoneA = new ContinuumZone(
                    edge.A,
                    edge.B,
                    dirAB * offsetA,
                    radiusA,
                    dirAB);

                var zoneB = new ContinuumZone(
                    edge.B,
                    edge.A,
                    dirBA * offsetB,
                    radiusB,
                    dirBA);

                AddZone(edge.A, zoneA);
                AddZone(edge.B, zoneB);
            }
        }

        private void AddZone(int systemIndex, in ContinuumZone zone)
        {
            if (!_zonesBySystem.TryGetValue(systemIndex, out var list))
            {
                list = new List<ContinuumZone>(4);
                _zonesBySystem[systemIndex] = list;
            }

            list.Add(zone);
        }

        private static float ComputeZoneRadius(in StarSys system)
        {
            // Радиус зоны берём минимум из константы и радиуса последней орбиты/звезды
            return ContinuumConsts.EntryZoneRadius;
        }

        private static float ComputeZoneOffset(in StarSys system)
        {
            // Орбиты — фиксированные слоты 1..OrbitSlots, даже если планет нет.
            float outerOrbitUnits = StarSysemConstants.OrbitSlots * StarSysemConstants.PlanetOrbitUnit;
            float starRadius = system.Star.radius;
            return Mathf.Max(outerOrbitUnits, starRadius) + ContinuumConsts.EntryZoneOffset;
        }

        private static bool IsValidSystemIndex(int index, StarSys[] galaxy)
        {
            return index >= 0 && index < galaxy.Length;
        }

        private void TryPlaceArrivedShip(in ContinuumTransit transit, in SimulationStepContext context)
        {
            var gameState = context.GameState;
            var galaxy = gameState?.Galaxy;
            if (galaxy == null)
                return;

            int toIndex = transit.ToSystemIndex;
            int fromIndex = transit.FromSystemIndex;
            if (!IsValidSystemIndex(toIndex, galaxy) || !IsValidSystemIndex(fromIndex, galaxy))
                return;

            var targetSys = galaxy[toIndex];
            var fromSys = galaxy[fromIndex];

            var runtime = targetSys.State ?? new LocalSysRuntimeContext();
            var ship = transit.Ship;

            Vector3 dirBA = (fromSys.GalaxyPosition - targetSys.GalaxyPosition).normalized;
            if (dirBA.sqrMagnitude <= 0f)
                dirBA = Vector3.up;

            float radius = GetOrbitRadius(targetSys, 3);
            ship.Position = dirBA * radius;
            ship.CurrentSpeed = ShipSpeed.GetMetricMaxSpeed(in ship); // После прибытия корабль движется внутри системы в метрике.
            runtime.Ships.Add(ship);

            targetSys.State = runtime;
            galaxy[toIndex] = targetSys;
        }

        private static float GetOrbitRadius(in StarSys system, int orbitIndex)
        {
            var orbits = system.PlanetOrbits;
            if (orbits != null && orbits.Length >= orbitIndex && orbits[orbitIndex - 1] > 0f)
                return orbits[orbitIndex - 1] * StarSysemConstants.PlanetOrbitUnit;

            return Mathf.Max(system.Star.radius, ContinuumConsts.EntryZoneOffset * orbitIndex);
        }
    }
}
