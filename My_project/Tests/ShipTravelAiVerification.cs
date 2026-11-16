using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

public static class ShipTravelAiVerification
{
    private static Assembly Runtime;
    private static object LocalAi, LocalMover, GlobalAi, GlobalMover;
    private static Type LocalContext, GlobalContext;
    private static int Checks;

    public static int Run(Assembly runtime)
    {
        Runtime = runtime;
        LocalAi = New("_Project.Scripts.Simulation.Local.Stages.Ai.LocalAiStage");
        LocalMover = New("_Project.Scripts.Simulation.Local.Stages.Movement.ShipMover");
        GlobalAi = New("_Project.Scripts.Simulation.Global.Stages.Ai.GlobalAiStage");
        GlobalMover = New("_Project.Scripts.Simulation.Global.Stages.Movement.GlobalMovementStage");
        LocalContext = T("_Project.Scripts.Simulation.Local.LocalSimulationContext");
        GlobalContext = T("_Project.Scripts.Simulation.Core.SimulationStepContext");

        VerifyMove(false, 1, "WarpToPoint");
        VerifyMove(true, 1, "WarpToPoint");
        VerifyMove(false, 0.4f, "FlyToPoint");
        VerifyMove(true, 0.4f, "FlyToPoint");
        VerifyTurnGate();
        VerifySimulationSwitch();
        VerifyPatrol();
        VerifyTrade(false);
        VerifyTrade(true);
        return Checks;
    }

    private static void VerifyMove(bool global, float distance, string expectedFirstTask)
    {
        var ship = Ship();
        var system = System(ship);
        var state = State(system);
        Order(ship, "MoveToPoint", Vector3.up * distance, 0.0001f, true);
        var tasks = new List<string>();
        var phases = new List<string>();
        object lastExecution = null;
        bool done = false;
        float step = global ? 2f : 0.02f;
        int ticksPerTurn = global ? 1 : 100;
        double flyStart = -1;
        for (int tick = 0; tick < 3000; tick++)
        {
            Tick(ship, system, state, global, tick / ticksPerTurn, tick % ticksPerTurn == 0, step);
            var execution = F(F(ship, "Ai"), "TaskExecution");
            if (execution != null && !ReferenceEquals(lastExecution, execution))
            {
                string task = P(P(execution, "Task"), "Type").ToString();
                tasks.Add(task);
                lastExecution = execution;
                if (task == "FlyToPoint") flyStart = tick * (double)step;
            }
            if (tick % ticksPerTurn == 0) phases.Add(F(F(ship, "Warp"), "Phase").ToString());
            if ((bool)P(F(F(ship, "Ai"), "CurrentOrder"), "IsEmpty")) { done = true; break; }
        }
        string label = global ? "global" : "local";
        Check(done && Vector3.Distance((Vector3)F(ship, "Position"), Vector3.up * distance) <= 0.0001f, label + " AI reaches destination and completes order");
        Check(tasks.Count > 0 && tasks[0] == expectedFirstTask, label + " behavior chooses " + expectedFirstTask);
        if (expectedFirstTask == "WarpToPoint")
        {
            Check(string.Join(",", tasks) == "WarpToPoint,FlyToPoint", label + " warp is followed by exactly one metric approach");
            Check(phases.Count >= 6 && string.Join(",", phases.GetRange(0, 6)) == "Charging,Charging,Charging,Warp,MetricBrake,Metric", label + " phases occupy exactly three charge turns and one brake turn");
            Check(Math.Abs(flyStart - 10) < 0.001, label + " metric approach starts immediately on brake completion boundary");
        }
    }

    private static void VerifyTurnGate()
    {
        var ship = Ship();
        var system = System(ship);
        var state = State(system);
        Order(ship, "MoveToPoint", Vector3.up, 0.0001f, true);
        Tick(ship, system, state, false, 0, false, 0.02f);
        Check(F(F(ship, "Ai"), "TaskExecution") == null && F(F(ship, "Warp"), "Phase").ToString() == "Metric", "No AI task or charge starts between turn boundaries");
        Tick(ship, system, state, false, 1, true, 0.02f);
        Check(F(F(ship, "Warp"), "Phase").ToString() == "Charging", "Deferred decision starts at next turn boundary");
    }

    private static void VerifyPatrol()
    {
        var ship = Ship();
        var system = System(ship);
        var state = State(system);
        Order(ship, "Patrol", Vector3.up * 2, 0.2f, 0.0001f);
        var tasks = new List<object>();
        object lastExecution = null;
        for (int turn = 0; turn < 30 && tasks.Count < 3; turn++)
        {
            Tick(ship, system, state, true, turn, true, 2f);
            var execution = F(F(ship, "Ai"), "TaskExecution");
            if (execution != null && !ReferenceEquals(lastExecution, execution))
            {
                tasks.Add(P(execution, "Task"));
                lastExecution = execution;
            }
        }
        Check(tasks.Count >= 3 && P(tasks[0], "Type").ToString() == "WarpToPoint" && P(tasks[1], "Type").ToString() == "FlyToPoint", "Patrol uses common warp and approach behavior");
        Check(P(tasks[0], "Destination").Equals(P(tasks[1], "Destination")) && !P(tasks[1], "Destination").Equals(P(tasks[2], "Destination")), "Patrol selects another point only after completing the whole route");
        Check(F(F(F(ship, "Ai"), "CurrentOrder"), "Type").ToString() == "Patrol", "Initial order assigner preserves active patrol");
    }

    private static void VerifySimulationSwitch()
    {
        var ship = Ship();
        var system = System(ship);
        var state = State(system);
        Order(ship, "MoveToPoint", Vector3.up, 0.0001f, true);
        var phases = new List<string>();
        for (int tick = 0; tick < 200; tick++)
        {
            Tick(ship, system, state, false, tick / 100, tick % 100 == 0, 0.02f);
            if (tick % 100 == 0) phases.Add(F(F(ship, "Warp"), "Phase").ToString());
        }
        for (int turn = 2; turn < 6; turn++)
        {
            Tick(ship, system, state, true, turn, true, 2f);
            phases.Add(F(F(ship, "Warp"), "Phase").ToString());
        }
        Check(string.Join(",", phases) == "Charging,Charging,Charging,Warp,MetricBrake,Metric", "Switching local to global preserves charge and brake turn counts");
        Check(P(P(F(F(ship, "Ai"), "TaskExecution"), "Task"), "Type").ToString() == "FlyToPoint", "Simulation switch retains the original route and metric approach");
    }

    private static void VerifyTrade(bool global)
    {
        var ship = Ship();
        var key = New("_Project.Items.ItemKey", E("_Project.Items.ItemType", "Item"), 1);
        var seller = Station(1, 1, key, true);
        var buyer = Station(2, 2, key, false);
        var stations = Array.CreateInstance(T("_Project.Scripts.Stations.Station"), 2);
        stations.SetValue(seller, 0); stations.SetValue(buyer, 1);
        var system = System(ship);
        Set(system, "Stations", stations);
        var state = State(system);
        Order(ship, "TradeInSystem", 0.0001f);
        var sequence = new List<string>();
        object lastExecution = null;
        bool sold = false;
        int ticksPerTurn = global ? 1 : 100;
        for (int tick = 0; tick < ticksPerTurn * 40; tick++)
        {
            Tick(ship, system, state, global, tick / ticksPerTurn, tick % ticksPerTurn == 0, global ? 2f : 0.02f);
            var execution = F(F(ship, "Ai"), "TaskExecution");
            if (execution != null && !ReferenceEquals(lastExecution, execution))
            {
                sequence.Add(P(P(execution, "Task"), "Type").ToString());
                lastExecution = execution;
                if (P(P(execution, "Task"), "Type").ToString() == "BuyAtStation")
                    Check(Vector3.Distance((Vector3)F(ship, "Position"), Vector3.up) <= 0.0001f, "Trade task is issued only after arrival");
            }
            InvokeStatic("_Project.Scripts.Simulation.Local.Stages.Interaction.ShipAiTradeTaskExecutor", "Process", system);
            if (execution != null && P(P(execution, "Task"), "Type").ToString() == "SellAtStation" && (bool)P(execution, "IsFinished"))
            {
                sold = P(P(execution, "Result"), "Outcome").ToString() == "Succeeded";
                break;
            }
        }
        string label = global ? "global" : "local";
        Check(sold, label + " trade completes purchase and sale");
        Check(string.Join(",", sequence) == "WarpToPoint,FlyToPoint,BuyAtStation,WarpToPoint,FlyToPoint,SellAtStation", label + " both trade legs use common navigation");
        Check((int)Call(P(buyer, "Cargo"), "GetAmount", key) == 10 && (int)P(F(ship, "Cargo"), "Used") == 0, label + " goods actually reach buyer");
    }

    private static void Tick(object ship, object system, object state, bool global, int turn, bool start, float delta)
    {
        if (global)
        {
            var context = Activator.CreateInstance(GlobalContext, state, turn, delta, E("_Project.Scripts.Core.GameState.ERunMode", "Auto"), null, -1, start);
            Call(GlobalAi, "Run", context);
            Call(GlobalMover, "Run", context);
        }
        else
        {
            var context = Activator.CreateInstance(LocalContext, state, system, turn, delta, start);
            Call(LocalAi, "Run", context);
            Call(LocalMover, "Run", context);
        }
    }

    private static object Ship()
    {
        var ship = FormatterServices.GetUninitializedObject(T("_Project.Scripts.Ships.Ship"));
        var stats = New("_Project.Scripts.Ships.ShipStats");
        Set(stats, "MetricSpeed", 50f); Set(stats, "WarpSpeed", 50f); Set(stats, "Agility", 3f); Set(stats, "Acceleration", 8f);
        Set(ship, "Stats", stats); Set(ship, "Position", Vector3.zero); Set(ship, "Rotation", Quaternion.identity); Set(ship, "CurrentSpeed", 0.05f);
        Set(ship, "Ai", New("_Project.Scripts.Simulation.AI.ShipAiRuntime"));
        Set(ship, "TaskState", T("_Project.Scripts.Simulation.Ships.ShipTaskStack").GetProperty("Default").GetValue(null));
        Set(ship, "Cargo", New("_Project.Items.Cargo", 200));
        Set(ship, "MakerFraction", Fraction());
        return ship;
    }

    private static object System(object ship)
    {
        var system = New("_Project.Scripts.Galaxy.Data.StarSys");
        var context = New("_Project.Scripts.Simulation.LocalSysRuntimeContext");
        ((IList)P(context, "Ships")).Add(ship);
        Set(system, "State", context);
        return system;
    }

    private static object State(object system)
    {
        var state = FormatterServices.GetUninitializedObject(T("_Project.Scripts.Core.GameState.GameStateService"));
        var galaxy = Array.CreateInstance(system.GetType(), 1);
        galaxy.SetValue(system, 0);
        Set(state, "_galaxy", galaxy); Set(state, "_activeLocalSystemIndex", -1);
        return state;
    }

    private static object Station(int id, float distance, object key, bool sells)
    {
        var station = New("_Project.Scripts.Stations.Station");
        Set(station, "Uid", New("_Project.Scripts.Core.UID", E("_Project.Scripts.Core.EntityType", "Station"), id));
        Set(station, "Position", Vector3.up * distance); Set(station, "Owner", Fraction());
        var storage = New("_Project.Scripts.Stations.StorageModuleState");
        if (sells) Call(F(storage, "Cargo"), "Add", key, 10);
        var trade = New("_Project.Scripts.Stations.TradeModuleState");
        var order = New("_Project.Scripts.Stations." + (sells ? "OrderSell" : "OrderBy"));
        Set(order, "Key", key); Set(order, "Amount", 10); Set(order, "Price", sells ? 2 : 4);
        ((IDictionary)F(trade, sells ? "OrdersSell" : "OrdersBuy")).Add(key, order);
        var modules = Array.CreateInstance(T("_Project.Scripts.Stations.StationModule"), 2);
        modules.SetValue(Module("Storage", storage), 0); modules.SetValue(Module("Trade", trade), 1);
        Set(station, "Modules", modules);
        return station;
    }

    private static object Fraction() => New("_Project.Scripts.NPC.Fraction.Fraction", 1, "test", 0, 0, null, E("EFractionTypes", "Regular"), 0L);
    private static object Module(string name, object state)
    {
        var module = New("_Project.Scripts.Stations.StationModule");
        Set(module, "Type", E("_Project.Scripts.Stations.EStationModuleType", name)); Set(module, "State", state);
        return module;
    }
    private static void Order(object ship, string method, params object[] parameters)
    {
        var args = new object[parameters.Length + 1]; args[0] = ship; Array.Copy(parameters, 0, args, 1, parameters.Length);
        InvokeStatic("_Project.Scripts.Simulation.AI.ShipAiOrders", method, args);
    }
    private static Type T(string name) => Runtime.GetType(name, true);
    private static object New(string name, params object[] args) => Activator.CreateInstance(T(name), args);
    private static object E(string name, string value) => Enum.Parse(T(name), value);
    private static object F(object value, string name) => value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(value);
    private static object P(object value, string name) => value.GetType().GetProperty(name).GetValue(value);
    private static void Set(object value, string name, object data) => value.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(value, data);
    private static object Call(object value, string method, params object[] args) => value.GetType().GetMethod(method).Invoke(value, args);
    private static object InvokeStatic(string type, string method, params object[] args) => T(type).GetMethod(method).Invoke(null, args);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Checks++;
        Debug.Log("TRAVEL_AI_CHECK_PASS " + message);
    }
}
