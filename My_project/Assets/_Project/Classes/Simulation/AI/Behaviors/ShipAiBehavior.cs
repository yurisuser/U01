using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Осмысленный шаг ИИ, который при необходимости выдаёт задачу Simulation.</summary>
    public abstract class ShipAiBehavior
    {
        public abstract bool TryCreateTask(in Ship ship, in StarSys system, out ShipAiTask task);

        public virtual bool IsCompletedBy(in ShipAiTaskResult result)
        {
            return true;
        }
    }
}
