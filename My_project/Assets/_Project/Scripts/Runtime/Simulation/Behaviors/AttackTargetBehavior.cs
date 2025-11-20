using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Core.Runtime;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.PilotMotivation;
using _Project.Scripts.Simulation.Primitives;
using UnityEngine;

namespace _Project.Scripts.Simulation.Behaviors
{
    /// <summary>Поведение атаки конкретной цели.</summary>
    internal static class AttackTargetBehavior
    {
        private const float DistanceTolerance = 1f; // Допуск по дистанции.
        private const float FrontConeAngleDeg = 70f; // Угол, определяющий фронт цели.

        // Маневрируем к выгодной точке и обрабатываем оружие.
        public static BehaviorExecutionResult Execute(
            ref Ship ship,
            ref PilotMotive motive,
            in PilotAction action,
            StarSystemState state,
            float dt,
            List<ShotEvent> shotEvents)
        {
            var attack = action.Parameters.Attack;
            var targetUid = attack.Target;

            if (!TargetingPrimitive.TryResolveTarget(state, in targetUid, out var targetSnapshot, out var targetSlot))
            {
                motive.ClearCurrentTarget();
                motive.CompleteCurrentAction();
                return BehaviorExecutionResult.TargetLostResult;
            }

            if (!attack.AllowFriendlyFire && !FractionRelations.IsHostile(ship.MakerFraction.Id, targetSnapshot.Fraction))
            {
                motive.ClearCurrentTarget();
                motive.CompleteCurrentAction();
                return BehaviorExecutionResult.TargetLostResult;
            }

            float desiredRange = attack.DesiredRange > 0f ? attack.DesiredRange : ComputeVolleyRange(in ship);
            if (desiredRange <= 0f)
                desiredRange = 1f;

            float distance = PositioningPrimitive.DistanceToTarget(ship.Position, targetSnapshot); // Расстояние до цели.

            // Определяем, входим ли мы в конус переднего обстрела цели.
            var targetForward = targetSnapshot.Velocity;
            if (targetForward.sqrMagnitude < 0.0001f)
                targetForward = Vector3.right;
            else
                targetForward.Normalize();

            var toSelf = ship.Position - targetSnapshot.Position;
            bool inEnemyFront = Vector3.Angle(targetForward, toSelf) <= FrontConeAngleDeg;

            // Выбираем точку: если мы в его фронте — выход на бок/хвост; иначе держим хвост.
            Vector3 desiredPoint;
            float sideSign = ((ship.Uid.Id ^ targetUid.Id) & 1) == 0 ? 1f : -1f;
            var sideDir = new Vector3(-targetForward.y, targetForward.x, 0f) * sideSign;

            if (inEnemyFront)
            {
                desiredPoint = targetSnapshot.Position - targetForward * desiredRange + sideDir * (desiredRange * 0.5f);
            }
            else if (distance > desiredRange + DistanceTolerance)
            {
                desiredPoint = targetSnapshot.Position - targetForward * desiredRange;
            }
            else
            {
                desiredPoint = PositioningPrimitive.ComputeOrbitPoint(ship.Uid, ship.Position, targetSnapshot, desiredRange);
            }

            float speed = ship.Stats.MaxSpeed > 0f ? ship.Stats.MaxSpeed : desiredRange;
            MoveToPosition.Execute(ref ship, desiredPoint, speed, desiredRange * 0.1f, dt, stopOnArrival: false); // Летим к выбранной точке.

            if (distance <= desiredRange + DistanceTolerance)
            {
                var buffer = state.ShipsBuffer;
                var targetShip = buffer[targetSlot];
                var combatResult = CombatPrimitive.ProcessWeapons(ref ship, ref targetShip, distance, shotEvents);

                if (combatResult.HasFired)
                    state.TryUpdateShip(targetSlot, in targetShip);

                if (combatResult.TargetDestroyed)
                {
                    motive.ClearCurrentTarget();
                    motive.CompleteCurrentAction();
                    return BehaviorExecutionResult.TargetLostResult;
                }
            }

            return BehaviorExecutionResult.None;
        }

        // Подбираем эффективную дистанцию залпа по самому короткому оружию.
        private static float ComputeVolleyRange(in Ship ship)
        {
            var weapons = ship.Equipment.Weapons;
            if (weapons.Count <= 0)
                return 0f;

            float minRange = float.PositiveInfinity;
            bool hasWeapon = false;

            for (int i = 0; i < weapons.Count; i++)
            {
                var slot = weapons.GetSlot(i);
                if (!slot.HasWeapon)
                    continue;

                hasWeapon = true;
                if (slot.Weapon.Range > 0f && slot.Weapon.Range < minRange)
                    minRange = slot.Weapon.Range;
            }

            if (!hasWeapon || float.IsInfinity(minRange))
                return 0f;

            return Mathf.Max(0.5f, minRange);
        }
    }
}
