using System; // для ThreadLocal, Random
using System.Threading; // для ThreadLocal

namespace _Project.Scripts.Ships
{
    public static class Rng // потокобезопасный генератор случайных чисел для симуляции (без UnityEngine.Random)
    {
        private static readonly ThreadLocal<Random> _rng = new ThreadLocal<Random>(
            () => new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)) // отдельный генератор на поток
        );

        public static float Range(float min, float max) // случайное float в [min, max)
        {
            var r = _rng.Value!; // берём генератор текущего потока
            return min + (float)r.NextDouble() * (max - min); // линейная интерполяция
        }
    }
}

