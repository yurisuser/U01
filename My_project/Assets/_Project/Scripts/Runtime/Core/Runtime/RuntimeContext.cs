using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Центральная точка сбора состояний объектов симуляции.
    /// </summary>
    public sealed class RuntimeContext
    {
        public GalaxyService Galaxy { get; } ///<summary>Статичная информация о системах.</summary>
        public SystemRegistry Systems { get; } ///<summary>Состояния систем и кораблей по системам.</summary>
        public ShipRegistry Ships { get; } ///<summary>Реестр всех кораблей.</summary>
        public PilotRegistry Pilots { get; } ///<summary>Состояния пилотов.</summary>
        public TaskScheduler Tasks { get; } ///<summary>Очередь задач/приказов.</summary>
        public OwnershipMap Ownership { get; } ///<summary>Привязка UID к системам и слотам.</summary>

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

        /// <summary>
        /// Полностью очищаем контекст.
        /// </summary>
        public void Reset()
        {
            Tasks.Reset();
            Ships.Reset();
            Pilots.Reset();
            Systems.Reset();
            Galaxy.Reset();
            Ownership.Reset();
        }

        /// <summary>
        /// Инициализируем галактику и системы.
        /// </summary>
        public void Initialize(StarSys[] generatedGalaxy)
        {
            Reset();
            Galaxy.Initialize(generatedGalaxy);
            Systems.Initialize(Galaxy);
        }
    }
}
