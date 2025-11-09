
using _Project.Scripts.NPC.Fraction;    // для Fraction
using UnityEngine;                      // для Vector3, Quaternion

namespace _Project.Scripts.Ships
{
    public struct Ship
    {
        public readonly Core.UID Uid;     // уникальный ID корабля
        public Core.UID PilotUid;         // ID пилота-NPC или пустой UID, если корабль брошенный
        public readonly Fraction MakerFraction; // фракция завода кораблей
        public readonly EShipType Type;   // тип корабля (Fighter, Trader и т.п.)
        public Vector3 Position;          // мировая позиция центра масс
        public Quaternion Rotation;       // мировая ориентация корабля
        public ShipStats Stats;           // базовые характеристики корабля (Hp, скорость, маневренность)
        public Vector3 Velocity;          // текущая линейная скорость (м/с в плоскости)
        public bool IsActive;             // активен ли корабль в мире
        public ShipEquipment Equipment;   // оборудование корабля (минимум: оружейные слоты)

        public Ship(                      // конструктор, инициализирующий все поля
            Core.UID uid,                 // уникальный ID
            Core.UID pilotUid,            // ID пилота
            Fraction fraction,            // фракция завода кораблей
            EShipType type,               // тип корабля
            Vector3 position,             // мировая позиция
            Quaternion rotation,          // ориентация
            int hp,                       // здоровье
            float maxSpeed,               // максимальная скорость
            float agility,                // маневренность
            bool isActive                 // активность
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
                MaxSpeed = maxSpeed,      // максимальная скорость
                Agility = agility         // маневренность
            };
            Velocity = Vector3.zero;
            IsActive = isActive;          // сохраняем активность
            Equipment = default;          // инициализируется позже (в ShipCreator)
        }
    }
}
