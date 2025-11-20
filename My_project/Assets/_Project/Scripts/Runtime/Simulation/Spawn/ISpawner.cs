namespace _Project.Scripts.Simulation.Spawn
{
    /// <summary>Контракт исполнителя намерения спауна.</summary>
    public interface ISpawner
    {
        bool TryExecute(in SpawnIntent intent);               // попытаться выполнить намерение
    }
}
