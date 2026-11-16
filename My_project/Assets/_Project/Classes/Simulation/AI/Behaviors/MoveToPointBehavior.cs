using UnityEngine;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Simulation.Ships.Movement;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Общее поведение достижения координаты для приказа, патруля и торговли.</summary>
    public sealed class MoveToPointBehavior : ShipAiBehavior
    {
        private readonly Vector3 _destination;
        private readonly float _tolerance;
        private readonly bool _keepSpeed;
        private bool _taskCreated;
        private EShipTravelMode _mode;
        private bool _warpCompleted;

        public MoveToPointBehavior(Vector3 destination, float tolerance, bool keepSpeed)
        {
            _destination = destination;
            _tolerance = tolerance;
            _keepSpeed = keepSpeed;
        }

        public override bool TryCreateTask(in _Project.Scripts.Ships.Ship ship, in _Project.Scripts.Galaxy.Data.StarSys system, float movementStepSeconds, out ShipAiTask task)
        {
            if (_taskCreated)
            {
                task = null;
                return false;
            }

            if (_mode == EShipTravelMode.None)
            {
                var move = new MoveToPointParams { Destination = _destination, Tolerance = _tolerance, KeepSpeed = _keepSpeed };
                var comparison = ShipTravelPlanner.Estimate(in ship, in move, movementStepSeconds);
                _mode = comparison.PreferredMode;
                if (_mode == EShipTravelMode.None)
                {
                    task = null;
                    return false; // Неполный или недопустимый прогноз не превращаем в произвольный приказ варпа.
                }
            }

            _taskCreated = true;
            task = _mode == EShipTravelMode.Warp && !_warpCompleted
                ? (ShipAiTask)new WarpToPointTask(_destination, _tolerance)
                : new FlyToPointTask(_destination, _tolerance, _keepSpeed);
            return true;
        }

        public override bool IsCompletedBy(in ShipAiTaskResult result)
        {
            if (_mode == EShipTravelMode.Warp && !_warpCompleted && result.Outcome == EShipAiTaskOutcome.Succeeded)
            {
                _warpCompleted = true;
                _taskCreated = false; // После варпа выполняем только метрический подход, без нового выбора маршрута.
                return false;
            }
            return true; // Прибытие или ошибка завершают это поведение; восстановление выбирает родитель.
        }
    }
}
