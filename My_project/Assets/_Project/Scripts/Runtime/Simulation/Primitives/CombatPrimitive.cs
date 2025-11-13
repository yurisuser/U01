using System.Collections.Generic;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation.Primitives
{
    internal static class CombatPrimitive
    {
        public static CombatResult ProcessWeapons(ref Ship attacker, ref Ship target, float distance, List<ShotEvent> shotEvents)
        {
            bool hasFired = false;
            bool targetDestroyed = false;

            ref var weapons = ref attacker.Equipment.Weapons;
            int count = weapons.Count;

            for (int i = 0; i < count; i++)
            {
                var slot = weapons.GetSlot(i);
                if (!slot.HasWeapon)
                    continue;

                var weapon = slot.Weapon;
                if (weapon.Range > 0f && distance > weapon.Range)
                {
                    weapons.SetSlot(i, in slot);
                    continue;
                }

                slot.ShotsAccumulator += weapon.Rate;
                int shots = (int)slot.ShotsAccumulator;
                if (shots <= 0)
                {
                    weapons.SetSlot(i, in slot);
                    continue;
                }

                slot.ShotsAccumulator -= shots;
                float damage = weapon.Damage * shots;
                ApplyDamage(ref target, damage);
                hasFired = true;

                shotEvents?.Add(new ShotEvent
                {
                    Shooter = attacker.Uid,
                    Target = target.Uid,
                    Bullet = weapon.Bullet,
                    ShotsFired = shots,
                    DamageDealt = damage,
                    Origin = attacker.Position,
                    TargetPosition = target.Position
                });

                weapons.SetSlot(i, in slot);

                if (!target.IsActive)
                {
                    targetDestroyed = true;
                    break;
                }
            }

            return new CombatResult(hasFired, targetDestroyed);
        }

        private static void ApplyDamage(ref Ship target, float damage)
        {
            if (damage <= 0f)
                return;

            var stats = target.Stats;
            stats.Hp = Mathf.Max(0, stats.Hp - Mathf.RoundToInt(damage));
            target.Stats = stats;

            if (target.Stats.Hp <= 0)
            {
                stats.Hp = 0;
                target.Stats = stats;
                target.IsActive = false;
                target.Velocity = Vector3.zero;
            }
        }
    }

    internal readonly struct CombatResult
    {
        public readonly bool HasFired;
        public readonly bool TargetDestroyed;

        public CombatResult(bool hasFired, bool targetDestroyed)
        {
            HasFired = hasFired;
            TargetDestroyed = targetDestroyed;
        }
    }
}
