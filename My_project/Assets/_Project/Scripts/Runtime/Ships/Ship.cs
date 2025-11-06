
using _Project.Scripts.NPC.Fraction;    // для Fraction
using UnityEngine;                      // для Vector3, Quaternion

namespace _Project.Scripts.Ships
{
    public struct Ship
    {
        public readonly Core.UID Uid;          // уникальный ID корабля
        public Core.UID PilotUid;              // ID пилота-NPC или пустой UID, если беспилотник
        public readonly Fraction MakerFraction; // фракция завода кораблей
        public readonly EShipType Type;   // тип корабля (Fighter, Trader и т.п.)
        public Vector3 Position;          // мировая позиция центра масс
        public Quaternion Rotation;       // мировая ориентация корабля
        public int Hp;                    // текущее здоровье
        public float MaxSpeed;            // максимальная крейсерская скорость
        public float Agility;             //маневренность
        public Vector3 Velocity;          // текущая линейная скорость (м/с в плоскости)
        public bool IsActive;             // активен ли корабль в мире

        public Ship(                      // конструктор, инициализирующий все поля
            Core.UID uid,                      // уникальный ID
            Core.UID pilotUid,                 // ID пилота
            Fraction fraction,            // фракция завода кораблей
            EShipType type,               // тип корабля
            Vector3 position,             // мировая позиция
            Quaternion rotation,          // ориентация
            int hp,                       // здоровье
            float maxSpeed,               // максимальная скорость
            float agility,
            bool isActive                 // активность
        )
        {
            Uid = uid;                    // присваиваем уникальный ID
            PilotUid = pilotUid;          // сохраняем пилота
            MakerFraction = fraction;          // сохраняем фракцию
            Type = type;                  // сохраняем тип корабля
            Position = position;          // сохраняем позицию
            Rotation = rotation;          // сохраняем ориентацию
            Hp = hp;                      // сохраняем здоровье
            MaxSpeed = maxSpeed;
            Agility = agility;
            Velocity = Vector3.zero;
            IsActive = isActive;          // сохраняем активность
        }
    }
}
