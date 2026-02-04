using System.Collections.Generic;
using _Project.Scripts.Const;
using _Project.Scripts.Core.GameState;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Core;
using UnityEngine;

namespace _Project.Scripts.Simulation.Continuum
{
    /// <summary>Сервис Continuum: хранит зоны и активные транзиты, тикается в глобальной симуляции.</summary>
    public sealed class ContinuumService
    {
        private readonly List<ContinuumTransit> _transits = new(32); // Активные прыжки
        private readonly Dictionary<int, List<ContinuumZone>> _zonesBySystem = new(); // Зоны на систему
        private int _cachedGalaxyVersion = -1; // Кэш для пересчёта зон
        private int _cachedHyperEdgeCount = -1;

        public IReadOnlyList<ContinuumTransit> Transits => _transits;
        public IReadOnlyDictionary<int, List<ContinuumZone>> ZonesBySystem => _zonesBySystem;

        /// <summary>Тик глобальной симуляции: уменьшает таймеры и шлёт события прибытия.</summary>
        public void Tick(in SimulationStepContext context)
        {
            if (context.IsPaused || context.GameState == null)
                return;

            EnsureZones(context.GameState);

            var eventBus = context.EventBus;
            for (int i = _transits.Count - 1; i >= 0; i--)
            {
                var transit = _transits[i];
                transit.RemainingTurns -= 1;

                if (transit.RemainingTurns <= 0)
                {
                    // TODO: фактическое перемещение корабля между системами добавим, когда появится связь с Ship/контекстом.
                    if (eventBus != null)
                    {
                        var evt = new SimulationEvent(
                            SimulationEventType.ShipArrived,
                            transit.ToSystemIndex,
                            context.Day,
                            transit.Ship.Uid);
                        eventBus.Add(in evt);
                    }

                    _transits.RemoveAt(i);
                    continue;
                }

                _transits[i] = transit;
            }
        }

        /// <summary>Поставить прыжок в очередь.</summary>
        public void Enqueue(in ContinuumTransit transit)
        {
            _transits.Add(transit);
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
                    ship.CurrentSpeed = ship.Stats.MaxSpeed;
                }
            }

            return ContinuumTransit.Create(
                ship,
                fromSystemIndex,
                toSystemIndex,
                ContinuumConsts.JumpDurationTurns);
        }

        /// <summary>Пересчитать зоны, если изменилась галактика или список гиперлинков.</summary>
        public void EnsureZones(GameStateService gameState)
        {
            var galaxy = gameState?.Galaxy;
            var edges = gameState?.HyperlinkEdges;

            if (galaxy == null || galaxy.Length == 0 || edges == null)
                return;

            int version = galaxy.Length;
            int edgesCount = edges.Length;
            if (version == _cachedGalaxyVersion && edgesCount == _cachedHyperEdgeCount)
                return;

            RebuildZones(galaxy, edges);
            _cachedGalaxyVersion = version;
            _cachedHyperEdgeCount = edgesCount;
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
            float outerOrbit = 0f;
            var orbits = system.PlanetOrbits;
            if (orbits != null && orbits.Length > 0)
            {
                for (int i = 0; i < orbits.Length; i++)
                    if (orbits[i] > outerOrbit)
                        outerOrbit = orbits[i];
            }
            else
            {
                outerOrbit = Mathf.Max(outerOrbit, system.Star.radius);
            }

            return outerOrbit + ContinuumConsts.EntryZoneOffset;
        }

        private static bool IsValidSystemIndex(int index, StarSys[] galaxy)
        {
            return index >= 0 && index < galaxy.Length;
        }
    }
}
