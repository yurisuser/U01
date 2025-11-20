namespace _Project.Scripts.Simulation.Spawn
{
    /// <summary>Минимальный исполнитель намерений спауна (пока заглушка).</summary>
    public sealed class Spawner : ISpawner
    {
        public bool TryExecute(in SpawnIntent intent)
        {
            // TODO: подключить SHIP- и NPC-creator’ы, реестры и т.п.
            // Пока — заглушка «не удалось», чтобы плановик мог ре-энкьюить.
            return false;
        }
    }
}
