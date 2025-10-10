namespace _Project.Scripts.Galaxy.Data
{
    public static class StarSysCreator
    {
        public static StarSys Create(StarSys starSys, Star star, PlanetSys[] planetSysArray, int[] planetOrbits)
        {
            starSys.Star = star;
            starSys.Id = star.id;
            starSys.Name = star.name;
            starSys.PlanetSysArr = planetSysArray;
            starSys.PlanetOrbits = planetOrbits;
            return starSys;
        }
    }
}