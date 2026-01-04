using _Project.Scripts.Galaxy.Data;

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

            for (int i = 0; i < galaxy.Length; i++)
            {
                if (galaxy[i].ConstellationId != fraction.HomeSector)
                    continue;

                var sys = galaxy[i];
                sys.OwnerFrac = fraction;
                galaxy[i] = sys;
            }
        }
    }
}
