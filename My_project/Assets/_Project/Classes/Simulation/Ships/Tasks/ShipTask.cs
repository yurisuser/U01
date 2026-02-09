namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTask
    {
        public ShipTask(EShipTaskType type, ShipTaskParams parameters)
        {
            Type = type;
            Params = parameters;
        }

        public EShipTaskType Type;
        public ShipTaskParams Params;
    }
}
