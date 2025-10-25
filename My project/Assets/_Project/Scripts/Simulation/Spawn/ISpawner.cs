namespace _Project.Scripts.Simulation.Spawn
{
    public interface ISpawner
    {
        bool TryExecute(in SpawnIntent intent);               // попытаться выполнить намерение
    }
}