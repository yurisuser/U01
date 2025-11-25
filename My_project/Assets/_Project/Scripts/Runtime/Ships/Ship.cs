
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

    /// <summary>Фиксированный буфер опорных точек пути (value-тип, без ссылок).</summary>
    public struct ShipPathSamples
    {
        public const int Capacity = 8;

        public ShipPathSample S0;
        public ShipPathSample S1;
        public ShipPathSample S2;
        public ShipPathSample S3;
        public ShipPathSample S4;
        public ShipPathSample S5;
        public ShipPathSample S6;
        public ShipPathSample S7;
        public int Count;

        public void Clear()
        {
            Count = 0;
        }

        public bool TryAdd(in ShipPathSample sample)
        {
            if (Count >= Capacity)
                return false;
            SetSlot(Count, sample);
            Count++;
            return true;
        }

        public ShipPathSample GetAt(int index)
        {
            return GetSlot(index);
        }

        private ShipPathSample GetSlot(int index)
        {
            switch (index)
            {
                case 0: return S0;
                case 1: return S1;
                case 2: return S2;
                case 3: return S3;
                case 4: return S4;
                case 5: return S5;
                case 6: return S6;
                case 7: return S7;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }

        private void SetSlot(int index, in ShipPathSample sample)
        {
            switch (index)
            {
                case 0: S0 = sample; break;
                case 1: S1 = sample; break;
                case 2: S2 = sample; break;
                case 3: S3 = sample; break;
                case 4: S4 = sample; break;
                case 5: S5 = sample; break;
                case 6: S6 = sample; break;
                case 7: S7 = sample; break;
                default: throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }

    /// <summary>Снэпшот данных корабля в мире.</summary>
    public struct Ship
    {
        public const int PathSampleCapacity = ShipPathSamples.Capacity; // сколько опорных точек пути храним на шаг

        public readonly Core.UID Uid;     // уникальный ID корабля
        public Core.UID PilotUid;         // ID пилота-NPC или пустой UID, если корабль брошенный
        public readonly Fraction MakerFraction; // фракция завода кораблей
        public readonly EShipType Type;   // тип корабля (Fighter, Trader и т.п.)
        public Vector3 Position;          // мировая позиция центра масс
        public Quaternion Rotation;       // мировая ориентация корабля
        public ShipStats Stats;           // базовые характеристики корабля (Hp, скорость, маневренность)
        public bool IsActive;             // активен ли корабль в мире
        public ShipEquipment Equipment;   // оборудование корабля (минимум: оружейные слоты)
        public ShipPathSamples Path;      // опорные точки пути на текущий шаг

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
                MaxSpeed = 50,            // максимальная скорость
                Agility = 30              // маневренность
            };
            IsActive = isActive;          // сохраняем активность
            Equipment = default;          // инициализируется позже (в ShipCreator)
            Path = default;
            Path.Clear();
        }
    }
}
