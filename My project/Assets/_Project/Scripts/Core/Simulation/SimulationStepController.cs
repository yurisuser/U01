using _Project.Scripts.Core;                    // доступ к Core.Core.GameState
using _Project.Scripts.Core.GameState;          // ERunMode
using UnityEngine;                             // для Debug.Log

namespace _Project.Scripts.Core.Simulation
{
    /// <summary>
    /// Единая точка тика логики по таймеру (без MonoBehaviour).
    /// Core вызывает UpdateStep(dt); внутри выполняется ровно один логический шаг, когда накопится интервал.
    /// </summary>
    public sealed class SimulationStepController
    {
        private float _logicTimer = 0f;         // накопитель времени между логическими тиками (сек)

        /// <summary>
        /// Вызывать из Core.Update() и передавать Time.deltaTime.
        /// </summary>
        public void UpdateStep(float dt)        // dt — прошедшее время в секундах за кадр
        {
            var s = Core.GameState.Current; // снимок состояния на этот кадр

            if (s.RunMode == ERunMode.Auto)      // в авто-режиме тикаем по таймеру
            {
                _logicTimer += dt;               // копим прошедшее время
                if (_logicTimer >= s.LogicStepSeconds) // набрали длительность логического шага
                {
                    DoLogicStep();               // выполнить один логический шаг (конвейер добавим позже)
                    Core.GameState.AdvanceTick(); // увеличиваем индекс тика
                    _logicTimer -= s.LogicStepSeconds; // вычитаем интервал, оставляя «хвост» для точности
                }
            }
            else
            {
                _logicTimer = 0f;                // в паузе таймер не копим, чтобы не «взрывался» после паузы
            }
        }

        private void DoLogicStep()               // тестовый шаг: здесь позже будет конвейер Inputs→…→Snapshot
        {
            UnityEngine.Debug.Log($"Logic tick: {Core.GameState.Current.TickIndex}");
        }
    }
}