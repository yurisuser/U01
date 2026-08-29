using _Project.Scripts.Const;
using _Project.Scripts.Core;
using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Actions;
using _Project.Scripts.Simulation.Ships;
using _Project.Scripts.Stations;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Trade.Models;
using UnityEngine;

namespace _Project.Scripts.Simulation.Local.Stages.Interaction
{
    /// <summary>Локальные взаимодействия (стыковка/ремонт).</summary>
    public sealed class LocalInteractionStage : ILocalSimulationStage
    {
        public void Run(in LocalSimulationContext context)
        {
            if (!context.HasActiveSystem || context.GameState == null)
                return;

            var gameState = context.GameState;
            var galaxy = gameState.Galaxy;
            if (galaxy == null || galaxy.Length == 0)
                return;

            int index = gameState.ActiveLocalSystemIndex;
            if (index < 0 || index >= galaxy.Length)
                return;

            var system = galaxy[index];
            if (system.State == null || system.Stations == null || system.Stations.Length == 0)
                return;

            ShipAiTradeTaskExecutor.Process(ref system);
            DockingInteraction.ProcessDockActions(ref system);
            TradeInteraction.ProcessTradeActions(ref system);
            DockingInteraction.ProcessUndockActions(ref system);
            galaxy[index] = system;
        }
    }
}
