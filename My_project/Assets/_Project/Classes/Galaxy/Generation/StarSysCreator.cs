using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class StarSysCreator
    {
        public static StarSys Create(StarSys starSys, Star star, PlanetSys[] planetSysArray, int[] planetOrbits)
        {
            starSys.Star = star;
            starSys.Uid = star.Uid;
            starSys.DisplayName = star.Name;
            starSys.PlanetSysArr = planetSysArray;
            starSys.PlanetOrbits = planetOrbits;
            return starSys;
        }
    }
}
