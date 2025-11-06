
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
        public float Agility;             //маневренность
        public bool IsActive;             // активен ли корабль в мире

        public Ship(                      // конструктор, инициализирующий все поля
            Core.UID uid,                      // уникальный ID
            Core.UID pilotUid,                 // ID пилота
            Fraction fraction,            // фракция завода кораблей
            EShipType type,               // тип корабля
            Vector3 position,             // мировая позиция
            Quaternion rotation,          // ориентация
            int hp,                       // здоровье
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
            Agility = agility;
            IsActive = isActive;          // сохраняем активность
        }
    }
}
