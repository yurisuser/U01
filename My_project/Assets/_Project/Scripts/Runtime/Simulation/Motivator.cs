using System;
using System.Threading;
using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.Simulation.PilotMotivation;

namespace _Project.Scripts.Simulation
{
    /// <summary>Высокоуровневый API для назначения мотивов пилотам и развёртывания приказов в действия.</summary>
    public sealed class Motivator
    {
        private static int _seedCounter = Environment.TickCount; // Счётчик для генерации случайных патрулей.

        private readonly float _defaultPatrolRadius; // Радиус патруля по умолчанию.
        private readonly float _defaultPatrolSpeed; // Скорость патруля по умолчанию.
        private readonly float _arriveDistance; // Радиус прибытия.

        // Настраиваем параметры, которые будут использоваться при создании мотивов.
        public Motivator(float defaultPatrolRadius, float arriveDistance, float defaultPatrolSpeed)
        {
            _defaultPatrolRadius = Mathf.Max(arriveDistance, defaultPatrolRadius);
            _defaultPatrolSpeed = Mathf.Max(0.1f, defaultPatrolSpeed);
            _arriveDistance = Mathf.Max(0.01f, arriveDistance);
        }

        // Создаём патруль вокруг точки с дефолтными параметрами.
        public PilotMotive CreateDefaultPatrol(Vector3 origin, float desiredSpeedOverride = float.NaN)
        {
            var speed = float.IsNaN(desiredSpeedOverride)
                ? _defaultPatrolSpeed
                : Mathf.Max(0.0f, desiredSpeedOverride);

            return CreatePatrol(origin, _defaultPatrolRadius, speed, origin);
        }

        // Создаём патруль с указанными параметрами.
        public PilotMotive CreatePatrol(Vector3 center, float radius, float desiredSpeed, Vector3 origin)
        {
            var motive = new PilotMotive();
            ConfigurePatrol(ref motive, center, radius, desiredSpeed);
            motive.EnsurePatrolAction(origin);
            return motive;
        }

        // Создаём мотив атаки конкретной цели.
        public PilotMotive CreateAttackTarget(UID target, float desiredRange, bool allowFriendlyFire = false)
        {
            var motive = new PilotMotive();
            ConfigureAttackTarget(ref motive, target, desiredRange, allowFriendlyFire);
            return motive;
        }

        // Создаём мотив атаки любого врага.
        public PilotMotive CreateAttackAll(float searchRadius, bool allowFriendlyFire = false)
        {
            var motive = new PilotMotive();
            ConfigureAttackAll(ref motive, searchRadius, allowFriendlyFire);
            return motive;
        }

        // Конфигурируем режим патруля для существующего мотива.
        public void ConfigurePatrol(ref PilotMotive motive, Vector3 center, float radius, float desiredSpeed)
        {
            var clampedRadius = Mathf.Max(radius, _arriveDistance * 2f);
            var clampedSpeed = Mathf.Max(0.1f, desiredSpeed);
            var actionParam = new ActionParam
            {
                Coordinates = center,
                Distance = clampedRadius
            };

            motive.SetOrder(EPilotOrder.Patrol, in actionParam);
            motive.ConfigurePatrol(center, clampedRadius, clampedSpeed, _arriveDistance, CreateSeed());
        }

        // Конфигурируем атаку конкретной цели.
        public void ConfigureAttackTarget(ref PilotMotive motive, UID target, float desiredRange, bool allowFriendlyFire = false)
        {
            var clampedRange = Mathf.Max(0.1f, desiredRange);
            var param = new ActionParam
            {
                Target = target,
                DesiredRange = clampedRange,
                AllowFriendlyFire = allowFriendlyFire
            };

            motive.SetOrder(EPilotOrder.AttackTarget, in param);
        }

        // Конфигурируем атаку всех врагов.
        public void ConfigureAttackAll(ref PilotMotive motive, float searchRadius, bool allowFriendlyFire = false)
        {
            var clampedRadius = Mathf.Max(0.1f, searchRadius);
            var param = new ActionParam
            {
                Distance = clampedRadius,
                AllowFriendlyFire = allowFriendlyFire
            };

            motive.SetOrder(EPilotOrder.AttackAllEnemies, in param);
        }

        // Обновляем стек действий в зависимости от приказа.
        public void Update(ref PilotMotive motive, Vector3 origin)
        {
            switch (motive.Order)
            {
                case EPilotOrder.Patrol:
                    motive.EnsurePatrolAction(origin);
                    break;
                case EPilotOrder.AttackTarget:
                    motive.EnsureAttackTargetAction();
                    break;
                case EPilotOrder.AttackAllEnemies:
                    motive.EnsureAttackAllFlow();
                    break;
            }
        }

        // Сообщаем мотиватору, что действие завершено.
        public void OnActionCompleted(ref PilotMotive motive, Vector3 origin)
        {
            switch (motive.Order)
            {
                case EPilotOrder.Patrol:
                    motive.EnsurePatrolAction(origin);
                    break;
                case EPilotOrder.AttackTarget:
                    motive.EnsureAttackTargetAction();
                    break;
                case EPilotOrder.AttackAllEnemies:
                    motive.EnsureAttackAllFlow();
                    break;
            }
        }

        // Генерируем семя для патруля.
        private static uint CreateSeed()
        {
            var value = unchecked((uint)Interlocked.Increment(ref _seedCounter));
            return value == 0u ? 0xA511E9B3u : value;
        }
    }
}
