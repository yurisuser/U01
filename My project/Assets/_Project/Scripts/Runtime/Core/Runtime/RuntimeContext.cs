using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.Runtime
{
    /// <summary>
    /// Central access point that aggregates runtime services.
    /// </summary>
    public sealed class RuntimeContext
    {
        public GalaxyService Galaxy { get; }
        public SystemRegistry Systems { get; }
        public FleetRegistry Fleets { get; }
        public TaskScheduler Tasks { get; }
        public OwnershipMap Ownership { get; }

        public RuntimeContext()
        {
            Ownership = new OwnershipMap();
            Galaxy = new GalaxyService();
            Systems = new SystemRegistry();
            Fleets = new FleetRegistry(Systems, Ownership);
            Tasks = new TaskScheduler();
        }

        public void Reset()
        {
            Tasks.Reset();
            Fleets.Reset();
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
