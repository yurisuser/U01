using _Project.Scripts.Galaxy.Data;
using _Project.Scripts.Ships;
using _Project.Scripts.Stations;
using _Project.Trade;
using _Project.Items;

namespace _Project.Scripts.Simulation.AI
{
    /// <summary>Торговля одним товаром между станциями текущей системы.</summary>
    public sealed class TradeInSystemBehavior : ShipAiBehavior
    {
        private enum Phase
        {
            FindRoute,
            WaitBuy,
            SellAtBuyer,
            WaitSell,
            FindNextBuyer,
        }

        private readonly float _tolerance;
        private Phase _phase;
        private _Project.Scripts.Core.UID _sellerUid;
        private _Project.Scripts.Core.UID _buyerUid;
        private ItemKey _key;
        private int _desiredAmount;
        private MoveToPointBehavior _travel;
        private StationTradeTask _stationTask;

        public TradeInSystemBehavior(float tolerance)
        {
            _tolerance = tolerance;
            _phase = Phase.FindRoute;
        }

        public override bool TryCreateTask(in Ship ship, in StarSys system, float movementStepSeconds, out ShipAiTask task)
        {
            if (_travel != null)
                return _travel.TryCreateTask(in ship, in system, movementStepSeconds, out task);
            if (_stationTask != null)
            {
                task = _stationTask;
                _stationTask = null; // Торговую задачу выдаём после успешного прибытия к станции.
                return true;
            }

            bool created;
            switch (_phase)
            {
                case Phase.FindRoute:
                    created = TryCreateBuyTask(in ship, in system, out task);
                    break;

                case Phase.SellAtBuyer:
                    created = TryCreateSellTask(in ship, in system, out task);
                    break;

                case Phase.FindNextBuyer:
                    created = TryCreateNextBuyerTask(in ship, in system, out task);
                    break;

                default:
                    task = null;
                    return false;
            }

            if (!created)
                return false;
            _stationTask = (StationTradeTask)task;
            _travel = new MoveToPointBehavior(_stationTask.StationPosition, _stationTask.Tolerance, keepSpeed: false);
            return _travel.TryCreateTask(in ship, in system, movementStepSeconds, out task);
        }

        public override bool IsCompletedBy(in ShipAiTaskResult result)
        {
            if (_travel != null)
            {
                if (!_travel.IsCompletedBy(in result))
                    return false; // Метрический подход ещё впереди.
                _travel = null;
                if (result.Outcome == EShipAiTaskOutcome.Succeeded)
                    return false; // Теперь можно выдать сохранённую задачу покупки или продажи.
                _stationTask = null; // Ошибка маршрута обрабатывается так же, как неудача соответствующего плеча сделки.
            }

            if (_phase == Phase.WaitBuy)
            {
                _phase = result.Outcome == EShipAiTaskOutcome.Succeeded && result.Amount > 0
                    ? Phase.SellAtBuyer
                    : Phase.FindRoute;
            }
            else if (_phase == Phase.WaitSell)
            {
                _phase = result.Outcome == EShipAiTaskOutcome.Succeeded && result.Amount >= _desiredAmount
                    ? Phase.FindRoute
                    : Phase.FindNextBuyer;
            }

            return false;
        }

        private bool TryCreateBuyTask(in Ship ship, in StarSys system, out ShipAiTask task)
        {
            task = null;
            if (ship.Cargo.Used > 0 || !SearchTradeService.TryFindBestInSystem(system, out TradeRoute route))
                return false;

            if (!TryGetStation(in system, route.SellerUid, out var seller) || !TryGetStation(in system, route.BuyerUid, out var buyer))
                return false;

            int amount = FitToCargo(route.Amount, ship.Cargo.Capacity, ship.Cargo.Used);
            if (amount <= 0)
                return false;

            _sellerUid = seller.Uid;
            _buyerUid = buyer.Uid;
            _key = route.Key;
            _desiredAmount = amount;
            _phase = Phase.WaitBuy;
            task = new BuyAtStationTask(_sellerUid, seller.Position, _key, amount, _tolerance);
            return true;
        }

        private bool TryCreateSellTask(in Ship ship, in StarSys system, out ShipAiTask task)
        {
            task = null;
            if (!TryGetStation(in system, _buyerUid, out var buyer))
            {
                _phase = Phase.FindNextBuyer;
                return false;
            }

            int amount = ship.Cargo.GetAmount(_key);
            if (amount <= 0)
            {
                _phase = Phase.FindRoute;
                return false;
            }

            _desiredAmount = amount;
            _phase = Phase.WaitSell;
            task = new SellAtStationTask(buyer.Uid, buyer.Position, _key, amount, _tolerance);
            return true;
        }

        private bool TryCreateNextBuyerTask(in Ship ship, in StarSys system, out ShipAiTask task)
        {
            task = null;
            if (ship.Cargo.GetAmount(_key) <= 0)
            {
                _phase = Phase.FindRoute;
                return false;
            }

            if (!SearchTradeService.TryFindBestBuyerInSystem(system, _key, out _buyerUid))
                return false;

            _phase = Phase.SellAtBuyer;
            return TryCreateSellTask(in ship, in system, out task);
        }

        private static int FitToCargo(int amount, int capacity, int used)
        {
            if (capacity <= 0)
                return amount;

            int free = capacity - used;
            return free <= 0 ? 0 : (amount < free ? amount : free);
        }

        private static bool TryGetStation(in StarSys system, _Project.Scripts.Core.UID stationUid, out Station station)
        {
            if (system.Stations != null)
            {
                for (int i = 0; i < system.Stations.Length; i++)
                {
                    if (system.Stations[i].Uid.Id == stationUid.Id)
                    {
                        station = system.Stations[i];
                        return true;
                    }
                }
            }

            station = default;
            return false;
        }
    }
}
