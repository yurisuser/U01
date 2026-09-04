
using System;
using _Project.Items;
using _Project.Scripts.NPC.Fraction;    // для Fraction
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Simulation.AI;
using _Project.Trade;
using UnityEngine;                      // для Vector3, Quaternion

namespace _Project.Scripts.Ships
{
    public sealed class Ship
        : ITradeActor
    {
        public readonly Core.UID Uid;     // уникальный ID корабля
        public Core.UID PilotUid;         // ID пилота-NPC или пустой UID, если корабль брошенный
        public readonly Fraction MakerFraction; // фракция завода кораблей
        public readonly EShipType Type;   // тип корабля (Fighter, Trader и т.п.)
        public Vector3 Position;          // мировая позиция центра масс
        public Quaternion Rotation;       // мировая ориентация корабля
        public ShipStats Stats;           // базовые характеристики корабля (Hp, скорость, маневренность)
        public float PrefabSize;          // масштаб префаба
        public string PrefabKey;          // ключ префаба
        public float CurrentSpeed;        // текущая скорость (ед/сек)
        public bool IsActive;             // активен ли корабль в мире
        public InstalledEquip Equipment;  // установленное оборудование корабля
        public ShipTaskStack TaskState;   // задачи корабля
        public ShipAction CurrentAction;  // текущее действие (стыковка/торговля и т.д.)
        public EShipActionFailReason LastActionFailReason; // причина последнего сбоя действия
        public TopShipOrder TopOrder;     // верхний приказ
        public Cargo Cargo;               // содержимое и вместимость трюма
        public ShipAiRuntime Ai;          // новое состояние ИИ корабля

        public Fraction Owner => MakerFraction;

        Cargo ITradeActor.Cargo => Cargo;

        long ITradeActor.Money => MakerFraction.Money;

        /// <summary>Конструктор, инициализирующий все поля корабля.</summary>
        public Ship(                      // конструктор, инициализирующий все поля
            Core.UID uid,                 // уникальный ID
            Core.UID pilotUid,            // ID пилота
            Fraction fraction,            // фракция завода кораблей
            EShipType type,               // тип корабля
            Vector3 position,             // мировая позиция
            Quaternion rotation,          // ориентация
            int hp,                       // здоровье
            float maxSpeed,               // максимальная скорость
            float agility,                // маневренность (сколько радиан за ход может повернуть корабль)
            bool isActive,                // активность
            float acceleration,           // ускорение/торможение
            float prefabSize,             // масштаб префаба
            string prefabKey,             // ключ префаба
            int cargo                    // вместимость трюма
        )
        {
            Uid = uid;                    // присваиваем уникальный ID
            PilotUid = pilotUid;          // сохраняем пилота
            MakerFraction = fraction;     // сохраняем фракцию
            Type = type;                  // сохраняем тип корабля
            Position = position;          // сохраняем позицию
            Rotation = rotation;          // сохраняем ориентацию
            Stats = new ShipStats         // сохраняем характеристики в отдельную структуру
            {
                Hp = hp,                  // здоровье
                MaxSpeed = maxSpeed,      // максимальная скорость берём из каталога
                Agility = agility,        // маневренность берём из каталога
                Acceleration = acceleration // ускорение/торможение берём из каталога
            };
            CurrentSpeed = 0f;            // начальная скорость
            IsActive = isActive;          // сохраняем активность
            PrefabSize = prefabSize;
            PrefabKey = prefabKey;
            Equipment = default;          // инициализация позже
            TaskState = ShipTaskStack.Default;
            CurrentAction = default;
            LastActionFailReason = EShipActionFailReason.None;
            TopOrder = default;
            Cargo = new Cargo(cargo);
            Ai = new ShipAiRuntime();
        }

        /// <summary>Клонирование для снапшотов (поверхностное, с общими ссылками).</summary>
        public Ship Clone()
        {
            return (Ship)MemberwiseClone();
        }
    }
}
