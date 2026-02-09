namespace _Project.Scripts.Simulation.Ships
{
    public static partial class ShipTaskBuilder
    {
        public static ShipTask JumpToSystem(int targetSystemIndex)
        {
            return new ShipTask(EShipTaskType.JumpToSystem, new ShipTaskParams
            {
                TypeTask = EShipTaskType.JumpToSystem,
                JumpToSystemParams = new JumpToSystemParams
                {
                    TargetSystemIndex = targetSystemIndex
                }
            });
        }
    }
}
