using System;
using UnityEngine;
using _Project.Scripts.Core;

namespace _Project.Scripts.Simulation.PilotMotivation
{
    // Описывает состояние мотивации пилота и раскладывает приказы на действия.
    public struct PilotMotive
    {
        private EPilotOrder _order; // Текущий приказ для пилота.
        private ActionParam _orderParam; // Параметры текущего приказа.
        private PilotActionStack _actions; // Очередь действий, которую нужно выполнить.
        private PatrolState _patrol; // Состояние патрулирования.
        private bool _initialized; // Флаг инициализации внутренних структур.

        public EPilotOrder Order => _order; // Активный приказ.
        public ActionParam OrderParameters => _orderParam; // Доступные параметры приказа.
        public int ActionCount => _actions.Count; // Сколько действий готово к выполнению.
        public bool IsInitialized => _initialized; // Проверка первичной настройки.
        public UID CurrentTarget => _orderParam.Target; // UID текущей цели.
        internal bool HasCurrentTarget => IsValidUid(_orderParam.Target); // Есть ли назначенная цель.

        // Сбрасываем состояние и готовим стек действий.
        public void Reset(int actionCapacity = 16)
        {
            _actions.Initialize(actionCapacity);
            _order = EPilotOrder.Idle;
            _orderParam = default;
            _patrol = default;
            _initialized = true;
        }

        // Назначаем новый приказ и очищаем текущие действия.
        public void SetOrder(EPilotOrder order, in ActionParam param, int actionCapacity = 16)
        {
            CheckInitialized(actionCapacity);
            _order = order;
            _orderParam = param;
            _actions.Clear();
            _patrol = default;
        }

        // Подсматриваем действие без удаления.
        public bool TryPeekAction(out PilotAction action)
        {
            CheckInitialized();
            return _actions.TryPeek(out action);
        }

        // Достаём верхнее действие.
        public bool TryPopAction(out PilotAction action)
        {
            CheckInitialized();
            return _actions.TryPop(out action);
        }

        // Завершаем текущее действие и обновляем состояние, если нужно.
        public void CompleteCurrentAction()
        {
            if (!TryPopAction(out _))
                return;

            if (_order == EPilotOrder.Patrol)
            {
                var patrol = _patrol;
                patrol.HasTarget = false;
                _patrol = patrol;
            }
        }

        // Настраиваем параметры патруля.
        internal void ConfigurePatrol(Vector3 center, float radius, float desiredSpeed, float arriveDistance, uint randomState)
        {
            CheckInitialized();

            var patrolRadius = Math.Max(radius, arriveDistance * 2f);
            _patrol = new PatrolState
            {
                Center = center,
                Radius = patrolRadius,
                DesiredSpeed = Math.Max(0.1f, desiredSpeed),
                ArriveDistance = Math.Max(0.01f, arriveDistance),
                RandomState = randomState,
                CurrentTarget = Vector3.zero,
                HasTarget = false
            };
        }

        // Гарантируем, что в стеке есть цель патруля и движение к ней.
        internal bool EnsurePatrolAction(Vector3 origin)
        {
            if (_order != EPilotOrder.Patrol)
                return false;

            EnsurePatrolTarget(origin);

            var desired = PilotAction.CreateMoveTo(
                _patrol.CurrentTarget,
                _patrol.DesiredSpeed,
                _patrol.ArriveDistance);

            if (_actions.Count == 0)
            {
                _actions.Push(in desired);
                return true;
            }

            if (_actions.TryPeek(out var current))
            {
                if (current.Action != desired.Action ||
                    (current.Parameters.Move.Destination - desired.Parameters.Move.Destination).sqrMagnitude > 0.0001f ||
                    Math.Abs(current.Parameters.Move.DesiredSpeed - desired.Parameters.Move.DesiredSpeed) > 0.0001f ||
                    Math.Abs(current.Parameters.Move.ArriveDistance - desired.Parameters.Move.ArriveDistance) > 0.0001f)
                {
                    _actions.ReplaceTop(in desired);
                }

                return true;
            }

            _actions.Push(in desired);
            return true;
        }

        // Сохраняем текущую цель в приказе.
        internal void SetCurrentTarget(in UID target)
        {
            _orderParam.Target = target;
        }

        // Сбрасываем текущую цель.
        internal void ClearCurrentTarget()
        {
            _orderParam.Target = default;
        }

        // Убеждаемся, что атака конкретной цели присутствует в действиях.
        internal bool EnsureAttackTargetAction()
        {
            if (_order != EPilotOrder.AttackTarget && _order != EPilotOrder.AttackAllEnemies)
                return false;

            if (!HasCurrentTarget)
                return false;

            var desired = PilotAction.CreateAttackTarget(
                _orderParam.Target,
                _orderParam.DesiredRange,
                _orderParam.AllowFriendlyFire);

            return EnsureOrReplaceAction(in desired);
        }

        // Добавляем поиск цели, если приказ атаковать всех.
        internal bool EnsureAcquireAction()
        {
            if (_order != EPilotOrder.AttackAllEnemies)
                return false;

            var desired = PilotAction.CreateAcquireTarget(
                _orderParam.Distance,
                _orderParam.AllowFriendlyFire);

            return EnsureOrReplaceAction(in desired);
        }

        // Общий поток для приказа атаки всех: либо атакуем текущую, либо ищем новую.
        internal bool EnsureAttackAllFlow()
        {
            if (_order != EPilotOrder.AttackAllEnemies)
                return false;

            return HasCurrentTarget ? EnsureAttackTargetAction() : EnsureAcquireAction();
        }

        // Подбираем новую точку патруля, если достигли текущей.
        private void EnsurePatrolTarget(Vector3 origin)
        {
            var patrol = _patrol;
            if (!patrol.HasTarget)
            {
                AssignNextPatrolTarget(ref patrol, origin);
                _patrol = patrol;
                return;
            }

            var toTarget = patrol.CurrentTarget - origin;
            if (toTarget.sqrMagnitude <= patrol.ArriveDistance * patrol.ArriveDistance)
            {
                AssignNextPatrolTarget(ref patrol, origin);
                _patrol = patrol;
            }
        }

        // Выбираем следующую точку патруля в пределах радиуса.
        private static void AssignNextPatrolTarget(ref PatrolState patrol, Vector3 origin)
        {
            var center = patrol.Center;
            if (!patrol.HasTarget && center == Vector3.zero)
                center = origin;

            var randomState = patrol.RandomState;
            var next = PickPointWithinRadius(ref randomState, center, patrol.Radius);
            var minDistanceSqr = patrol.ArriveDistance * patrol.ArriveDistance;
            for (int i = 0; i < 5 && (next - origin).sqrMagnitude < minDistanceSqr; i++)
                next = PickPointWithinRadius(ref randomState, center, patrol.Radius);

            patrol.Center = center;
            patrol.RandomState = randomState;
            patrol.CurrentTarget = next;
            patrol.HasTarget = true;
        }

        // Сэмплируем точку на диске радиуса патруля.
        private static Vector3 PickPointWithinRadius(ref uint state, Vector3 center, float radius)
        {
            var offset = SamplePointOnDisk(ref state, radius);
            return new Vector3(center.x + offset.x, center.y + offset.y, center.z);
        }

        // Генерируем смещение внутри круга с равномерным распределением.
        private static Vector2 SamplePointOnDisk(ref uint state, float radius)
        {
            float angle = NextFloat(ref state) * Mathf.PI * 2f;
            float distance = Mathf.Sqrt(NextFloat(ref state)) * radius;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
        }

        // Псевдослучайное число в [0,1).
        private static float NextFloat(ref uint state)
        {
            state = NextState(state);
            return (state & 0x00FFFFFFu) / 16777216f;
        }

        // Обновляем состояние генератора случайных чисел.
        private static uint NextState(uint state)
        {
            if (state == 0u)
                state = 0x9E3779B9u;

            state ^= state << 13;
            state ^= state >> 17;
            state ^= state << 5;
            return state;
        }

        // Убеждаемся, что структура инициализирована перед использованием.
        private void CheckInitialized(int actionCapacity = 16)
        {
            if (!_initialized)
                Reset(actionCapacity);
        }

        // Добавляем действие, заменяя верхнее при необходимости.
        private bool EnsureOrReplaceAction(in PilotAction desired)
        {
            CheckInitialized();

            if (_actions.Count == 0)
            {
                _actions.Push(in desired);
                return true;
            }

            if (_actions.TryPeek(out var current))
            {
                if (AreActionsEquivalent(in current, in desired))
                    return true;

                _actions.ReplaceTop(in desired);
                return true;
            }

            _actions.Push(in desired);
            return true;
        }

        // Сравниваем два действия по их параметрам.
        private static bool AreActionsEquivalent(in PilotAction current, in PilotAction desired)
        {
            if (current.Action != desired.Action)
                return false;

            switch (current.Action)
            {
                case EAction.MoveToCoordinates:
                    return (current.Parameters.Move.Destination - desired.Parameters.Move.Destination).sqrMagnitude <= 0.0001f &&
                           Math.Abs(current.Parameters.Move.DesiredSpeed - desired.Parameters.Move.DesiredSpeed) <= 0.0001f &&
                           Math.Abs(current.Parameters.Move.ArriveDistance - desired.Parameters.Move.ArriveDistance) <= 0.0001f;
                case EAction.AttackTarget:
                    return current.Parameters.Attack.Target.Id == desired.Parameters.Attack.Target.Id &&
                           current.Parameters.Attack.Target.Type == desired.Parameters.Attack.Target.Type &&
                           Math.Abs(current.Parameters.Attack.DesiredRange - desired.Parameters.Attack.DesiredRange) <= 0.0001f &&
                           current.Parameters.Attack.AllowFriendlyFire == desired.Parameters.Attack.AllowFriendlyFire;
                case EAction.AcquireTarget:
                    return Math.Abs(current.Parameters.Acquire.SearchRadius - desired.Parameters.Acquire.SearchRadius) <= 0.0001f &&
                           current.Parameters.Acquire.AllowFriendlyFire == desired.Parameters.Acquire.AllowFriendlyFire;
                default:
                    return false;
            }
        }

        // Проверяем, что UID не пустой.
        private static bool IsValidUid(in UID uid)
        {
            return uid.Id != 0;
        }

        private struct PatrolState
        {
            public Vector3 Center; // Центр патрулирования.
            public Vector3 CurrentTarget; // Сюда движемся сейчас.
            public float Radius; // Радиус патруля.
            public float DesiredSpeed; // Желаемая скорость на маршруте.
            public float ArriveDistance; // Радиус прибытия.
            public uint RandomState; // Состояние генератора случайных точек.
            public bool HasTarget; // Флаг наличия активной точки.
        }
    }
}
