using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;

namespace _Project.Scripts.Core.GameState
{
    /// <summary>Снимок данных для UI с тройным буфером кораблей.</summary>
    public struct RenderSnapshot
    {
        public ERunMode       RunMode; // Состояние симуляции.
        public EPlayStepSpeed PlayStepSpeed; // Выбранная скорость.
        public long           TickIndex; // Текущий тик.
        public float          LogicStepSeconds; // Длительность шага.
        public StarSys[]      Galaxy; // Ссылка на исходные данные галактики.
        public int            SelectedSystemIndex; // Активная система.
        public Ship[]         PreviousShips; // Корабли на прошлый шаг.
        public int            PreviousShipCount; // Сколько кораблей в previous.
        public Ship[]         CurrentShips; // Корабли текущего снапшота.
        public int            CurrentShipCount; // Их количество.
        public Ship[]         NextShips; // Буфер следующего шага.
        public int            NextShipCount; // Количество в next.
        public int            ShipsVersion; // Версия, чтобы UI понимал обновления.
        public float          StepProgress; // Прогресс между снапшотами.
    }
}
