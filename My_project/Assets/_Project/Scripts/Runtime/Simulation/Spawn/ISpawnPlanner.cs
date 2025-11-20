namespace _Project.Scripts.Simulation.Spawn
{
    /// <summary>Контракт планировщика намерений спауна.</summary>
    public interface ISpawnPlanner
    {
        void Tick(int tickIndex, float dt);                   // пересчитать потребности
        bool TryDequeueNextIntent(out SpawnIntent intent);    // выдать следующее намерение (если есть)
        void Requeue(in SpawnIntent intent);                  // вернуть назад, если исполнение не удалось
    }
}
