namespace _Project.Scripts.Core.Runtime
{
    /// <summary>Заготовка планировщика задач для систем/пилотов.</summary>
    public sealed class TaskScheduler
    {
        /// <summary>Сбрасывает будущие очереди задач.</summary>
        public void Reset()
        {
            // Пока никаких очередей — задел на будущее.
        }

        /// <summary>Периодическое обновление планировщика.</summary>
        public void Tick(float dt)
        {
            // Заглушка: логика появится при реализации задач.
        }
    }
}
