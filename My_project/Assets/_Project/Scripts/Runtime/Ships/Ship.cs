
using System;
using _Project.Scripts.NPC.Fraction;    // для Fraction
using UnityEngine;                      // для Vector3, Quaternion

namespace _Project.Scripts.Ships
{
    /// <summary>Опорная точка пути корабля внутри шага.</summary>
    public struct ShipPathSample
    {
        public Vector3 Position;   // позиция в нормализованном времени шага
        public Quaternion Rotation; // ориентация в нормализованном времени шага
        public float T;            // доля шага [0..1]
    }

    /// <summary>Снэпшот данных корабля в мире.</summary>
    public struct Ship
    {
        public const int PathSampleCapacity = 8; // сколько опорных точек пути храним на шаг

        public readonly Core.UID Uid;     // уникальный ID корабля
        public Core.UID PilotUid;         // ID пилота-NPC или пустой UID, если корабль брошенный
        public readonly Fraction MakerFraction; // фракция завода кораблей
        public readonly EShipType Type;   // тип корабля (Fighter, Trader и т.п.)
        public Vector3 Position;          // мировая позиция центра масс
        public Quaternion Rotation;       // мировая ориентация корабля
        public ShipStats Stats;           // базовые характеристики корабля (Hp, скорость, маневренность)
        public bool IsActive;             // активен ли корабль в мире
        public ShipEquipment Equipment;   // оборудование корабля (минимум: оружейные слоты)
        public ShipPathSample[] PathSamples; // опорные точки пути на текущий шаг
        public int PathSampleCount;          // сколько опорных точек заполнено

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
                MaxSpeed = 20,            // максимальная скорость
                Agility = 12              // маневренность
            };
            IsActive = isActive;          // сохраняем активность
            Equipment = default;          // инициализируется позже (в ShipCreator)
            PathSamples = new ShipPathSample[PathSampleCapacity];
            PathSampleCount = 0;
        }
    }
}
