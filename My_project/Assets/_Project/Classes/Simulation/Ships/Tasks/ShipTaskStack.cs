using System.Collections.Generic;

namespace _Project.Scripts.Simulation.Ships
{
    public struct ShipTaskStack
    {
        private Stack<ShipTask> _tasks;

        public static ShipTaskStack Default => new ShipTaskStack // стартовое состояние стека
        {
            _tasks = new Stack<ShipTask>(4),
        };

        public bool HasTasks => _tasks != null && _tasks.Count > 0; // быстрый флаг наличия задач

        public void PushTask(in ShipTask task) // добавить задачу на вершину
        {
            _tasks ??= new Stack<ShipTask>(4);
            _tasks.Push(task);
        }

        public bool TryPeek(out ShipTask task) // получить верхнюю задачу без снятия
        {
            if (_tasks == null || _tasks.Count == 0)
            {
                task = default;
                return false;
            }

            task = _tasks.Peek();
            return true;
        }

        public void Pop() // снять верхнюю задачу
        {
            if (_tasks == null || _tasks.Count == 0)
                return;
            _tasks.Pop();
        }

    }
}
