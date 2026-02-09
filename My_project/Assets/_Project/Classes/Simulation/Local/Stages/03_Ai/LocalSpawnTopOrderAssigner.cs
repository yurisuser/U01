using UnityEngine;
using _Project.Scripts.Ships;
using _Project.Scripts.Ships.Orders;
using _Project.Scripts.Simulation;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Назначение базового top-order для freshly spawned/idle кораблей.</summary>
    internal static class LocalSpawnTopOrderAssigner
    {
        public static void EnsureTradeInSystemOrders(LocalSysRuntimeContext runtime, int systemIndex, float patrolRadius)
        {
            var ships = runtime.Ships;
            for (int i = 0; i < ships.Count; i++)
            {
                var ship = ships[i];
                if (!ship.TopOrder.IsEmpty)
                    continue; // Не затираем приказы, выданные раньше.

                ship.TopOrder = new TopShipOrder
                {
                    Type = ETopShipOrderType.TradeInSystem,
                    Params = new TopShipOrderParams
                    {
                        Center = Vector3.zero,      // Пока торговля строится от центра системы.
                        Radius = patrolRadius,       // Допуск патруля/поиска внутри системы.
                        SystemIndex = systemIndex,   // Явная привязка приказа к системе.
                    }
                };

                ships[i] = ship; // value-type: записываем обратно в список.
            }
        }
    }
}
