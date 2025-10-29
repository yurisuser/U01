using _Project.Scripts.Core;
using _Project.Scripts.Core.GameState;


namespace _Project.Scripts.Simulation
{
    public sealed class Executor
    {
        private float _logicTimer = 0f;         // накопитель времени между логическими тиками (сек)

        public void UpdateStep(float dt)        // dt — прошедшее время в секундах за кадр
        {
            var stateService = GameBootstrap.GameState;
            var snapshot     = stateService.Current; // снимок состояния на этот кадр

            if (snapshot.RunMode == ERunMode.Auto)      // в авто-режиме тикаем по таймеру
            {
                _logicTimer += dt;                      // копим прошедшее время
                if (_logicTimer >= snapshot.LogicStepSeconds) // набрали длительность логического шага
                {
                    var nextSnapshot = snapshot;        // будущий снапшот для коммита
                    DoLogicStep(ref nextSnapshot);      // выполнить один логический шаг (конвейер добавим позже)
                    nextSnapshot.TickIndex++;           // увеличиваем индекс тика
                    stateService.Commit(nextSnapshot);  // запись состояния только на стадии Commit
                    _logicTimer -= snapshot.LogicStepSeconds; // вычитаем интервал, оставляя «хвост» для точности
                }
            }
            else
            {
                _logicTimer = 0f;                       // в паузе таймер не копим, чтобы не «взрывался» после паузы
            }
        }

        private void DoLogicStep(ref GameStateService.Snapshot snapshot) // тестовый шаг: здесь позже будет конвейер Inputs→…→Snapshot
        {
            UnityEngine.Debug.Log($"Logic tick: {snapshot.TickIndex}");
        }
    }
}
