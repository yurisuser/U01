using UnityEngine;

namespace _Project.Scripts.Stations
{
    /// <summary>Конфиг док-модуля.</summary>
    public sealed class DockModuleData : IStationModuleData
    {
        public int Slots; // число мест для стыковки
        public float DockingRange; // радиус, где можно начинать стыковку
        public Vector3[] Anchors; // локальные точки стыковки
        public EStationModuleType ModuleType => EStationModuleType.Dock; // идентификатор типа
    }
}
