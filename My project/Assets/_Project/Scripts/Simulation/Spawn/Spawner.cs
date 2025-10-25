namespace _Project.Scripts.Simulation.Spawn
{
    // Минимальный исполнитель: заглушка, вернёт true/false в зависимости от готовности
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