using System;
using UnityEngine; // можно убрать, если не хочешь зависимости

namespace _Project.Galaxy.Obj
{
    [Serializable]
    public struct Coordinates3d
    {
        public float X;
        public float Y;
        public float Z;

        public Coordinates3d(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        // Быстрая длина вектора
        public float Magnitude => Mathf.Sqrt(X * X + Y * Y + Z * Z);

        // Нормализация
        public Coordinates3d Normalized
        {
            get
            {
                var mag = Magnitude;
                return mag > 1e-5f ? new Coordinates3d(X / mag, Y / mag, Z / mag) : new Coordinates3d(0, 0, 0);
            }
        }

        // Оператор сложения
        public static Coordinates3d operator +(Coordinates3d a, Coordinates3d b) 
            => new Coordinates3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

        // Оператор вычитания
        public static Coordinates3d operator -(Coordinates3d a, Coordinates3d b) 
            => new Coordinates3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

        // Умножение на скаляр
        public static Coordinates3d operator *(Coordinates3d a, float d) 
            => new Coordinates3d(a.X * d, a.Y * d, a.Z * d);

        // Неявное преобразование в Vector3 (Unity)
        public static implicit operator Vector3(Coordinates3d c) 
            => new Vector3(c.X, c.Y, c.Z);

        // И обратно
        public static implicit operator Coordinates3d(Vector3 v) 
            => new Coordinates3d(v.x, v.y, v.z);

        public override string ToString() => $"({X:0.00}, {Y:0.00}, {Z:0.00})";
    }
}