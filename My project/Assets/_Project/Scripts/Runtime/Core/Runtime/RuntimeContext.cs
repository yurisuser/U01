using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Центральная точка сбора состояний обьектов
    /// </summary>
    public sealed class RuntimeContext
    {
        public GalaxyService Galaxy { get; }
        public SystemRegistry Systems { get; }
        public ShipRegistry Ships { get; }
        public PilotRegistry Pilots { get; }
        public TaskScheduler Tasks { get; }
        public OwnershipMap Ownership { get; }

        public RuntimeContext()
        {
            Ownership = new OwnershipMap();
            Galaxy = new GalaxyService();
            Systems = new SystemRegistry();
            Ships = new ShipRegistry(Systems, Ownership); // отдельный учет всех кораблей
            Pilots = new PilotRegistry();
            Tasks = new TaskScheduler();
        }

        public void Reset()
        {
            Tasks.Reset();
            Ships.Reset();
            Pilots.Reset();
            Systems.Reset();
            Galaxy.Reset();
            Ownership.Reset();
        }

        public void Initialize(StarSys[] generatedGalaxy)
        {
            Reset();
            Galaxy.Initialize(generatedGalaxy);
            Systems.Initialize(Galaxy);
        }
    }
}
