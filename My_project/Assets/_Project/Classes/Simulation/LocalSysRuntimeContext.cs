using System.Collections.Generic;
using UnityEngine;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation
{
    /// <summary>Динамическое состояние звёздной системы: корабли, снапшоты, ИИ-агенты.</summary>
    public sealed class LocalSysRuntimeContext
    {
        private List<Ship> _prevShipSnapshots = new(32);
        private List<Ship> _currShipSnapshots = new(32);
        private float _lastSnapshotTime;
        private float _lastSnapshotDuration = 0.0001f;

        /// <summary>Список кораблей в системе.</summary>
        public List<Ship> Ships { get; } = new List<Ship>(50);

        /// <summary>Снапшот предыдущего шага для интерполяции.</summary>
        public IReadOnlyList<Ship> PrevShipSnapshots => _prevShipSnapshots;

        /// <summary>Снапшот актуального шага для интерполяции.</summary>
        public IReadOnlyList<Ship> CurrShipSnapshots => _currShipSnapshots;

        /// <summary>Начать новый цикл подготовки снапшота: свапаем буферы и очищаем текущий.</summary>
        public void BeginShipSnapshot()
        {
            var tmp = _prevShipSnapshots;
            _prevShipSnapshots = _currShipSnapshots;
            _currShipSnapshots = tmp;
            _currShipSnapshots.Clear();
        }

        /// <summary>Скопировать живые корабли в снапшот и запомнить длительность шага.</summary>
        public void CommitShipSnapshot(float stepDuration, float commitTime)
        {
            _lastSnapshotDuration = Mathf.Max(0.0001f, stepDuration);
            _lastSnapshotTime = commitTime;

            CopyShips(Ships, _currShipSnapshots);
        }

        /// <summary>Коэффициент интерполяции для текущего момента времени.</summary>
        public float GetShipInterpolation(float currentTime)
        {
            if (_currShipSnapshots.Count == 0)
                return 0f;

            float t = (currentTime - _lastSnapshotTime) / Mathf.Max(0.0001f, _lastSnapshotDuration);
            return Mathf.Clamp01(t);
        }

        private static void CopyShips(List<Ship> source, List<Ship> target)
        {
            int count = source.Count;
            if (target.Capacity < count)
                target.Capacity = count;

            target.Clear();
            for (int i = 0; i < count; i++)
                target.Add(source[i]);
        }
    }
}
