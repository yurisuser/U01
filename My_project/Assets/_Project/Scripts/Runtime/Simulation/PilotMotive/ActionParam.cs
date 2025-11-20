namespace _Project.Scripts.Simulation.PilotMotivation
{
    using _Project.Scripts.Core;
    using UnityEngine;

    // Структура параметров высокого уровня для приказов пилота.
    public struct ActionParam
    {
        public Vector3 Coordinates; // Координаты цели или центра патруля.
        public UID SystemUID; // UID системы, если приказ системный.
        public UID Target; // Конкретная цель для атаки.
        public float Distance; // Дистанция патруля или поиска.
        public float DesiredRange; // Предпочтительная дистанция боёв.
        public bool AllowFriendlyFire; // Разрешён ли Friendly Fire.
    }
}
