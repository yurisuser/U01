using System;
using UnityEngine;

// можно убрать, если не хочешь зависимости

namespace _Project.Scripts.Galaxy.Data
{
    [Serializable]
    public struct Coordinates3d
    {
        public float x;
        public float y;
        public float z;

        public Coordinates3d(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        // Быстрая длина вектора
        public float Magnitude => Mathf.Sqrt(x * x + y * y + z * z);

        // Нормализация
        public Coordinates3d Normalized
        {
            get
            {
                var mag = Magnitude;
                return mag > 1e-5f ? new Coordinates3d(x / mag, y / mag, z / mag) : new Coordinates3d(0, 0, 0);
            }
        }

        // Оператор сложения
        public static Coordinates3d operator +(Coordinates3d a, Coordinates3d b) 
            => new Coordinates3d(a.x + b.x, a.y + b.y, a.z + b.z);

        // Оператор вычитания
        public static Coordinates3d operator -(Coordinates3d a, Coordinates3d b) 
            => new Coordinates3d(a.x - b.x, a.y - b.y, a.z - b.z);

        // Умножение на скаляр
        public static Coordinates3d operator *(Coordinates3d a, float d) 
            => new Coordinates3d(a.x * d, a.y * d, a.z * d);

        // Неявное преобразование в Vector3 (Unity)
        public static implicit operator Vector3(Coordinates3d c) 
            => new Vector3(c.x, c.y, c.z);

        // И обратно
        public static implicit operator Coordinates3d(Vector3 v) 
            => new Coordinates3d(v.x, v.y, v.z);

        public override string ToString() => $"({x:0.00}, {y:0.00}, {z:0.00})";
    }
}