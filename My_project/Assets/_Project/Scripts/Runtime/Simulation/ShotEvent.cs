using _Project.Scripts.Core;
using _Project.Scripts.Ships;
using UnityEngine;

namespace _Project.Scripts.Simulation
{
    /// <summary>
    /// Данные о произведённом выстреле — нужны визуалу/логам.
    /// </summary>
    public struct ShotEvent
    {
        public UID Shooter;
        public UID Target;
        public BulletSpec Bullet;
        public int ShotsFired;
        public float DamageDealt;
        public Vector3 Origin;
        public Vector3 TargetPosition;
    }
}
