using UnityEngine;

namespace _Project.Prefabs
{
    [CreateAssetMenu(fileName = "PrefabCatalog", menuName = "_Project/Rendering/Prefab Catalog")]
    public class PrefabCatalog : ScriptableObject
    {
        // ==== Системная карта ====
        public GameObject[] StarGalaxyPrefabsByType;    // индекс = (int)EStarType
        public GameObject[] StarSystemPrefabsByType;    // индекс = (int)EStarType
        public GameObject[] PlanetPrefabsByType;  // индекс = (int)EPlanetType
        public GameObject[] MoonPrefabsByType;    // индекс = (int)EMoonType
        public GameObject[] ShipPrefabsByClass;   // индекс = (int)EShipClass (добавим enum позже)

        // ==== Материалы, цвета, эффекты ====
        public Color OrbitPlanetColor;            // цвет линий орбит планет
        public Color OrbitMoonColor;              // цвет линий орбит лун

        // ==== Общие визуальные элементы ====
        public GameObject SelectionHighlightPrefab; // подсветка выбранного объекта и пр.
    }
}
