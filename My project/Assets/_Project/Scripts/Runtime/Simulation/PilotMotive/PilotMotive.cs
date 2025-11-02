using System.Collections.Generic;

namespace _Project.Scripts.Simulation.PilotMotive
{
    public struct PilotMotive
    {
        public EPilotOrder Order;
        public IOrderParam OrderParam;
        public Stack<IPilotAction> StackPilotActions;
    }
}
