namespace _Project.Scripts.Simulation.PilotMotive
{
    public enum EPilotOrder
    {
        Idle = 0,
        MoveToCoordinates = 1,
        MoveToSystem = 2,
        AttackTarget = 3,
        AttackAllEnemies = 4,
        Patrol = 5,
        DefendTarget = 6,
        DefendPosition = 7,
        Explore = 8
    }
}
