using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.NPC.Fraction.Create
{
    public static class FractionSpawner
    {
        public static void SpawnForFraction(StarSys[] galaxy, Fraction fraction)
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
