using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.Stations
{
    /// <summary>Создание станции из дефиниции.</summary>
    public static class StationCreator
    {
        public static Station Create(StationTypeDef def, Fraction owner, Vector3 position)
        {
            var uid = UIDService.Create(EntityType.Station); // генерим UID для станции
            var station = StationFactory.Create(def, uid, owner); // собираем станцию через фабрику
            station.Position = position; // позиция задаётся снаружи (по месту генерации)
            return station;
        }
    }
}
