using _Project.Industry.Recipes;
using _Project.Items;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Stations
{
    /// <summary>Тикает модули станции.</summary>
    public static class StationTickService
    {
        public static void TickTurn(in StarSys system, int turn)
        {
            if (system.Stations == null)
                return;

            for (int i = 0; i < system.Stations.Length; i++)
                TickIndustry(in system.Stations[i], turn);
        }

        private static void TickIndustry(in Station station, int turn)
        {
            if (station.Modules == null || station.Cargo == null)
                return;

            for (int i = 0; i < station.Modules.Length; i++)
            {
                var module = station.Modules[i];
                if (module == null ||
                    module.Type != EStationModuleType.Industry ||
                    module.Data is not IndustryModuleData data ||
                    module.State is not IndustryModuleState state ||
                    data.Recipe == null)
                    continue;

                TickRecipe(station.Cargo, data.Recipe, state, turn);
            }
        }

        private static void TickRecipe(Cargo cargo, Recipe recipe, IndustryModuleState state, int turn)
        {
            if (recipe.CycleTurns <= 0 || turn <= state.LastProductionTurn)
                return;

            state.ProductionProgressTurns += turn - state.LastProductionTurn;
            state.LastProductionTurn = turn;

            while (state.ProductionProgressTurns >= recipe.CycleTurns)
            {
                if (!CanCompleteCycle(cargo, recipe))
                {
                    state.ProductionProgressTurns = recipe.CycleTurns;
                    return;
                }

                ApplyStacks(cargo, recipe.Inputs, false);
                ApplyStacks(cargo, recipe.Outputs, true);
                state.ProductionProgressTurns -= recipe.CycleTurns;
            }
        }

        private static bool CanCompleteCycle(Cargo cargo, Recipe recipe)
        {
            long inputAmount = 0;
            if (recipe.Inputs != null)
            {
                for (int i = 0; i < recipe.Inputs.Length; i++)
                {
                    var input = recipe.Inputs[i];
                    if (input.Quantity <= 0)
                        continue;
                    if (cargo.GetAmount(input.Key) < input.Quantity)
                        return false;

                    inputAmount += input.Quantity;
                }
            }

            if (cargo.Capacity <= 0)
                return true;

            long outputAmount = 0;
            if (recipe.Outputs != null)
            {
                for (int i = 0; i < recipe.Outputs.Length; i++)
                    if (recipe.Outputs[i].Quantity > 0)
                        outputAmount += recipe.Outputs[i].Quantity;
            }

            return cargo.Used - inputAmount + outputAmount <= cargo.Capacity;
        }

        private static void ApplyStacks(Cargo cargo, ItemStack[] stacks, bool add)
        {
            if (stacks == null)
                return;

            for (int i = 0; i < stacks.Length; i++)
            {
                var stack = stacks[i];
                if (stack.Quantity <= 0 || stack.Key.IsEmpty)
                    continue;

                if (add)
                    cargo.Add(stack.Key, stack.Quantity);
                else
                    cargo.Remove(stack.Key, stack.Quantity);
            }
        }
    }
}
