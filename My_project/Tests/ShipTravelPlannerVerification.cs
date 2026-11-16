using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public static class TravelForecastVerification
{
    private static Assembly Runtime;
    private static Type ShipType, StatsType, MoveType;
    private static MethodInfo Estimate, MetricStep, WarpStep, Request;
    private static int Checks;
    private const float StepSeconds = 0.02f;

    public static void Run()
    {
        try
        {
            var args = Environment.GetCommandLineArgs();
            int argumentIndex = Array.IndexOf(args, "-u01TravelAssembly");
            if (argumentIndex < 0 || argumentIndex + 1 >= args.Length)
                throw new ArgumentException("Missing -u01TravelAssembly argument");
            Runtime = Assembly.LoadFrom(args[argumentIndex + 1]);
            ShipType = Runtime.GetType("_Project.Scripts.Ships.Ship", true);
            StatsType = Runtime.GetType("_Project.Scripts.Ships.ShipStats", true);
            MoveType = Runtime.GetType("_Project.Scripts.Simulation.Ships.MoveToPointParams", true);
            Estimate = Runtime.GetType("_Project.Scripts.Simulation.Ships.Movement.ShipTravelPlanner", true).GetMethod("Estimate");
            MetricStep = Runtime.GetType("_Project.Scripts.Simulation.Ships.Movement.ShipMetricMotion", true).GetMethod("Step");
            var processor = Runtime.GetType("_Project.Scripts.Simulation.Ships.Movement.ShipWarpProcessor", true);
            WarpStep = processor.GetMethod("Process");
            Request = processor.GetMethod("TryRequestWarp");

            var farShip = Ship(50, 50);
            var farMove = Move(1);
            var far = Compare(farShip, farMove);
            Check(Preferred(far) == "Warp", "Far target chooses warp");
            Near(Turns(far, "Metric"), 10, 0.03, "Metric time matches distance");
            Near(Turns(far, "Warp"), 6, 0.03, "Charge, rounded warp, brake and approach total six turns");
            var near = Compare(Ship(50, 50), Move(0.55f));
            Check(Preferred(near) == "Metric", "Near target chooses metric");
            Check(Status(Compare(Ship(50, 50), Move(0.4f)), "Warp") == "Unavailable", "Charge cannot pass warp exit point");
            var arrived = Compare(Ship(50, 50), Move(1, 10));
            Near(Turns(arrived, "Metric"), 0, 0, "Arrival tolerance is respected");
            Check(Preferred(arrived) == "Metric", "Already arrived does not charge warp");

            var unaligned = Ship(50, 50, 180, 0);
            var turning = Compare(unaligned, Move(2));
            Check((double)Field(Field(turning, "Warp"), "PreparationSeconds") >= 2, "Preparation waits for a turn boundary");
            Check(Preferred(Compare(Ship(50, 0.01f), Move(1))) == "Metric", "Independent slow warp speed changes choice");

            var overshootShip = Ship(300, 50);
            var overshoot = Compare(overshootShip, Move(5));
            Check(Status(overshoot, "Warp") == "Completed", "Brake overshoot remains reachable by metric return");
            Check((double)Field(Field(overshoot, "Warp"), "ApproachSeconds") > 0.4 / 0.3, "Brake overshoot includes actual turn and return");

            Near(Turns(far, "Warp") * 2, Replay(Ship(50, 50), farMove, true), 0.05, "Forecast matches tick-by-tick warp execution");
            Near(Turns(turning, "Warp") * 2, Replay(Ship(50, 50, 180, 0), Move(2), true), 0.05, "Preparation forecast matches execution");
            Near(Turns(overshoot, "Warp") * 2, Replay(Ship(300, 50), Move(5), true), 0.05, "Overshoot forecast matches execution");
            Near(Turns(near, "Metric") * 2, Replay(Ship(50, 50), Move(0.55f), false), 0.03, "Metric forecast matches execution");

            var coarseShip = Ship(50, 50, 130, 0, 0.2f, 0.001f);
            var coarseMove = Move(1);
            var coarse = Estimate.Invoke(null, new[] { coarseShip, coarseMove, (object)2f });
            Near(Turns(coarse, "Metric") * 2, ReplayGlobalMetric(coarseShip, coarseMove), 0.0001, "Forecast matches actual global task executor");
            var stoppingMove = Move(1, 0.002f);
            Set(stoppingMove, "KeepSpeed", false);
            var stopping = Compare(Ship(50, 0, 90, 0), stoppingMove);
            Near(Turns(stopping, "Metric") * 2, Replay(Ship(50, 0, 90, 0), stoppingMove, false), 0.03, "Stopping and course correction match execution");
            Check((double)Field(Field(Compare(Ship(50, 50, 4.9f), Move(2)), "Warp"), "PreparationSeconds") == 0, "Inside course tolerance charges immediately");
            Check((double)Field(Field(Compare(Ship(50, 50, 5.1f), Move(2)), "Warp"), "PreparationSeconds") >= 2, "Outside course tolerance waits for next turn");

            var stopped = Ship(50, 50, 0, 0, 0, 0);
            var immobile = Compare(stopped, Move(1));
            Check(Status(immobile, "Metric") == "Unavailable" && Status(immobile, "Warp") == "Unavailable", "No propulsion from rest is unavailable");
            Check(Status(Compare(Ship(50, 0), Move(1)), "Warp") == "Unavailable", "Zero warp speed is unavailable");
            Check(Status(Compare(Ship(0, 50, 0, 0), Move(1)), "Metric") == "Unavailable", "Zero metric speed is unavailable");

            var limited = Compare(Ship(50, 50), Move(1000));
            Check(Status(limited, "Metric") == "LowerBound", "Long route uses a proven metric lower bound");
            Check(Preferred(limited) == "Warp", "Warp is faster than the best possible metric route");
            var unknown = Compare(Ship(50, 0), Move(1000000));
            Check(Status(unknown, "Metric") == "StepLimitReached" && Preferred(unknown) == "None", "Unresolved route still reports uncertainty");
            var invalidMove = Move(1);
            Set(invalidMove, "Destination", new Vector3(float.NaN, 0, 0));
            Check(Status(Compare(Ship(50, 50), invalidMove), "Metric") == "InvalidInput", "NaN input rejected");

            var committed = Ship(50, 50);
            Request.Invoke(null, new[] { committed, (object)Vector3.up });
            Check(Status(Compare(committed, Move(2)), "Metric") == "InvalidInput", "Committed warp is not replanned");
            var beforePosition = Field(farShip, "Position");
            var beforeRotation = Field(farShip, "Rotation");
            var beforeSpeed = Field(farShip, "CurrentSpeed");
            var beforeWarp = Field(farShip, "Warp");
            Compare(farShip, farMove);
            Check(beforePosition.Equals(Field(farShip, "Position")) && beforeRotation.Equals(Field(farShip, "Rotation"))
                && beforeSpeed.Equals(Field(farShip, "CurrentSpeed")) && beforeWarp.Equals(Field(farShip, "Warp")), "Forecast leaves source ship unchanged");

            Parallel.For(0, 8, i => {
                var result = Compare(farShip, farMove);
                if (Turns(result, "Warp") != Turns(far, "Warp") || Turns(result, "Metric") != Turns(far, "Metric"))
                    throw new Exception("Parallel forecasts differ");
            });
            Check(true, "Parallel forecasts are deterministic");
            Checks += ShipTravelAiVerification.Run(Runtime);
            Debug.Log("TRAVEL_VERIFICATION_PASS checks=" + Checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogError("TRAVEL_VERIFICATION_FAIL " + error);
            EditorApplication.Exit(1);
        }
    }

    private static object Ship(float metric, float warp, float heading = 0, float currentSpeed = -1, float agility = 3, float acceleration = 8)
    {
        var ship = FormatterServices.GetUninitializedObject(ShipType);
        var stats = Activator.CreateInstance(StatsType);
        Set(stats, "MetricSpeed", metric); Set(stats, "WarpSpeed", warp);
        Set(stats, "Agility", agility); Set(stats, "Acceleration", acceleration);
        Set(ship, "Stats", stats); Set(ship, "Position", Vector3.zero);
        Set(ship, "Rotation", Quaternion.Euler(0, 0, heading));
        Set(ship, "CurrentSpeed", currentSpeed < 0 ? metric * 0.001f : currentSpeed);
        return ship;
    }

    private static object Move(float distance, float tolerance = 0.0001f)
    {
        var move = Activator.CreateInstance(MoveType);
        Set(move, "Destination", Vector3.up * distance); Set(move, "Tolerance", tolerance); Set(move, "KeepSpeed", true);
        return move;
    }

    private static double Replay(object ship, object move, bool warp)
    {
        if (warp) Request.Invoke(null, new[] { ship, Field(move, "Destination") });
        bool started = false;
        double elapsed = 0;
        for (int tick = 0; tick < 10000; tick++)
        {
            if (!warp && Arrived(ship, move)) return elapsed;
            bool handled = warp && (bool)WarpStep.Invoke(null, new[] { ship, (object)StepSeconds, tick % 100 == 0 });
            string phase = Field(Field(ship, "Warp"), "Phase").ToString();
            if (phase != "Metric") started = true;
            if (!handled)
            {
                if ((!warp || started) && Arrived(ship, move)) return elapsed;
                var parameters = Activator.CreateInstance(MoveType);
                Set(parameters, "Destination", Field(move, "Destination"));
                Set(parameters, "KeepSpeed", warp && !started || (bool)Field(move, "KeepSpeed"));
                Set(parameters, "Tolerance", warp && !started ? 0f : Field(move, "Tolerance"));
                MetricStep.Invoke(null, new[] { ship, parameters, (object)StepSeconds });
            }
            elapsed += StepSeconds;
            if ((!warp || started && phase == "Metric") && Arrived(ship, move)) return elapsed;
        }
        throw new Exception("Replay exceeded test horizon");
    }

    private static double ReplayGlobalMetric(object ship, object move)
    {
        var aiType = Runtime.GetType("_Project.Scripts.Simulation.AI.ShipAiRuntime", true);
            var taskType = Runtime.GetType("_Project.Scripts.Simulation.AI.FlyToPointTask", true);
        var executionType = Runtime.GetType("_Project.Scripts.Simulation.AI.ShipAiTaskExecution", true);
        var task = Activator.CreateInstance(taskType, Field(move, "Destination"), Field(move, "Tolerance"), Field(move, "KeepSpeed"));
        var execution = Activator.CreateInstance(executionType, task);
        var ai = Activator.CreateInstance(aiType);
        Set(ai, "TaskExecution", execution);
        Set(ship, "Ai", ai);
        var process = Runtime.GetType("_Project.Scripts.Simulation.Global.Stages.Movement.GlobalMovementStage", true)
            .GetMethod("ProcessMoveTo", BindingFlags.Static | BindingFlags.NonPublic);
        double elapsed = 0;
        for (int turn = 0; turn < 10000; turn++)
        {
            if (Arrived(ship, move)) return elapsed;
            process.Invoke(null, new[] { ship, (object)2f });
            elapsed += 2;
            if ((bool)executionType.GetProperty("IsFinished").GetValue(execution)) return elapsed;
        }
        throw new Exception("Global replay exceeded test horizon");
    }

    private static bool Arrived(object ship, object move) => Vector3.Distance((Vector3)Field(ship, "Position"), (Vector3)Field(move, "Destination")) <= (float)Field(move, "Tolerance");
    private static object Compare(object ship, object move) => Estimate.Invoke(null, new[] { ship, move, (object)StepSeconds });
    private static object Field(object value, string name) => value.GetType().GetField(name).GetValue(value);
    private static void Set(object value, string name, object data) => value.GetType().GetField(name).SetValue(value, data);
    private static string Preferred(object result) => result.GetType().GetProperty("PreferredMode").GetValue(result).ToString();
    private static string Status(object result, string route) => Field(Field(result, route), "Status").ToString();
    private static double Turns(object result, string route) => (double)Field(result, route).GetType().GetProperty("TotalTurns").GetValue(Field(result, route));
    private static void Near(double actual, double expected, double tolerance, string message) => Check(!double.IsNaN(actual) && Math.Abs(actual - expected) <= tolerance, message + " actual=" + actual + " expected=" + expected);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Checks++;
        Debug.Log("TRAVEL_CHECK_PASS " + message);
    }
}
