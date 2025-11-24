using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>Снимок логического состояния симуляции.</summary>
    public struct Snapshot
    {
        public ERunMode       RunMode; // Текущий режим (пауза/игра).
        public EPlayStepSpeed PlayStepSpeed; // Скорость воспроизведения.
        public long           TickIndex; // Номер шага.
        public float          LogicStepSeconds; // Длительность шага.
        public bool           RequestStep; // Запрос одиночного шага.
        public StarSys[]      Galaxy; // Копия данных галактики.
        public int            SelectedSystemIndex; // Выбранная система.
    }
}
