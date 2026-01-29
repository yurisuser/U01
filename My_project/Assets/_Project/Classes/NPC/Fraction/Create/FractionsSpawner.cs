using _Project.Scripts.Galaxy.Data;
using _Project.DataAccess;

namespace _Project.Scripts.NPC.Fraction.Create
{
    public static class FractionsSpawner
    {
        public static void SpawnAll(StarSys[] galaxy)
        {
            var fractions = FractionService.GetAll();
            for (int i = 0; i < fractions.Count; i++)
                SpawnForFraction(galaxy, fractions[i]);
        }

        private static void SpawnForFraction(StarSys[] galaxy, Fraction fraction)
        {
            if (galaxy == null || galaxy.Length == 0)
                return;

            if (fraction.HomeSector <= 0)
                return;

            CatalogFraction catalog = default;
            var hasCatalog = CATALOG.FractionsById != null
                && CATALOG.FractionsById.TryGetValue(fraction.Id, out catalog);
            var starNames = hasCatalog ? catalog.StarNames : null;
            var canRename = starNames != null && starNames.Count > 0;
            var nameIndex = 0;

            for (int i = 0; i < galaxy.Length; i++)
            {
                if (galaxy[i].ConstellationId != fraction.HomeSector)
                    continue;

                var sys = galaxy[i];
                sys.OwnerFrac = fraction;
                if (canRename && nameIndex < starNames.Count)
                {
                    var newName = starNames[nameIndex++];
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        sys.DisplayName = newName;
                        var star = sys.Star;
                        star.Name = newName;
                        sys.Star = star;

                        var planets = sys.PlanetSysArr;
                        if (planets != null)
                        {
                            for (int p = 0; p < planets.Length; p++)
                            {
                                var planetSys = planets[p];
                                var planet = planetSys.Planet;
                                planet.Name = StarNameCatalog.ComposePlanetName(newName, p);
                                planetSys.Planet = planet;

                                var moons = planetSys.Moons;
                                if (moons != null)
                                {
                                    for (int m = 0; m < moons.Length; m++)
                                    {
                                        var moon = moons[m];
                                        moon.Name = StarNameCatalog.ComposeMoonName(newName, p, m);
                                        moons[m] = moon;
                                    }
                                    planetSys.Moons = moons;
                                }

                                planets[p] = planetSys;
                            }

                            sys.PlanetSysArr = planets;
                        }
                    }
                }
                galaxy[i] = sys;
            }

            if (hasCatalog)
                FractionScenarioHandler.Apply(galaxy, fraction, catalog);
        }
    }
}
