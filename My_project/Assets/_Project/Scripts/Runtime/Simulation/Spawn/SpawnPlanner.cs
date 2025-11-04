using System.Collections.Generic;

namespace _Project.Scripts.Simulation.Spawn
{
    // Минимальный плановик: держит очередь намерений, сам ничего не генерирует (пока)
    public sealed class SpawnPlanner : ISpawnPlanner
    {
        private readonly Queue<SpawnIntent> _queue = new Queue<SpawnIntent>();

        public void Tick(int tickIndex, float dt)
        {
            // TODO: здесь появятся правила (например, "в каждой системе 1 патруль")
            // На MVP можно вручную класть намерения через EnqueueExternal(...)
        }

        public bool TryDequeueNextIntent(out SpawnIntent intent)
        {
            if (_queue.Count > 0)
            {
                intent = _queue.Dequeue();
                return true;
            }
            intent = default;
            return false;
        }

        public void Requeue(in SpawnIntent intent) => _queue.Enqueue(intent);

        // Вспомогательный метод для внешнего кода (квесты/тесты) — не обязателен
        public void EnqueueExternal(in SpawnIntent intent) => _queue.Enqueue(intent);
    }
}