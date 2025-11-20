using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.Runtime
{
    // Центральная точка сбора состояний объектов симуляции.
    public sealed class RuntimeContext
    {
        public GalaxyService Galaxy { get; } // Статичная информация о системах.
        public SystemRegistry Systems { get; } // Состояния систем и кораблей по системам.
        public ShipRegistry Ships { get; } // Реестр всех кораблей.
        public PilotRegistry Pilots { get; } // Состояния пилотов.
        public TaskScheduler Tasks { get; } // Очередь задач/приказов.
        public OwnershipMap Ownership { get; } // Привязка UID к системам и слотам.

        // Собираем все сервисы контекста.
        public RuntimeContext()
        {
            Ownership = new OwnershipMap();
            Galaxy = new GalaxyService();
            Systems = new SystemRegistry();
            Ships = new ShipRegistry(Systems, Ownership); // отдельный учет всех кораблей
            Pilots = new PilotRegistry();
            Tasks = new TaskScheduler();
        }

        // Полностью очищаем контекст.
        public void Reset()
        {
            Tasks.Reset();
            Ships.Reset();
            Pilots.Reset();
            Systems.Reset();
            Galaxy.Reset();
            Ownership.Reset();
        }

        // Инициализируем галактику и системы.
        public void Initialize(StarSys[] generatedGalaxy)
        {
            Reset();
            Galaxy.Initialize(generatedGalaxy);
            Systems.Initialize(Galaxy);
        }
    }
}
