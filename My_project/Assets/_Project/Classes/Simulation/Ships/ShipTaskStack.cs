using System.Collections.Generic;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTaskStack
    {
        private Stack<ShipTask> _tasks;

        public static ShipTaskStack Default => new ShipTaskStack
        {
            _tasks = new Stack<ShipTask>(4),
        };

        public bool HasTasks => _tasks != null && _tasks.Count > 0;

        public void PushTask(in ShipTask task)
        {
            _tasks ??= new Stack<ShipTask>(4);
            _tasks.Push(task);
        }

        public bool TryPeek(out ShipTask task)
        {
            if (_tasks == null || _tasks.Count == 0)
            {
                task = default;
                return false;
            }

            task = _tasks.Peek();
            return true;
        }

        public void Pop()
        {
            if (_tasks == null || _tasks.Count == 0)
                return;
            _tasks.Pop();
        }
    }
}
