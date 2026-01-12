using System.Collections.Generic;
using _Project.DataAccess;

namespace _Project.Scripts.NPC.Fraction
{
    public static class FractionService
    {
        private static Fraction[] _all;
        private static Dictionary<int, Fraction> _byId;

        public static IReadOnlyList<Fraction> GetAll() => _all ??= Load();

        public static void ReloadAll()
        {
            _all = Load();
            _byId = null;
        }

        public static bool TryGetById(int id, out Fraction fraction)
        {
            EnsureIndex();
            return _byId.TryGetValue(id, out fraction);
        }

        public static bool TryGetNameById(int id, out string name)
        {
            if (TryGetById(id, out var fraction))
            {
                name = fraction.Name;
                return true;
            }

            name = null;
            return false;
        }

        private static Fraction[] Load()
        {
            var list = CATALOG.Fractions;
            if (list == null || list.Count == 0)
                return new[] { new Fraction(0, "Default") };

            var result = new Fraction[list.Count];
            for (int i = 0; i < list.Count; i++)
                result[i] = new Fraction(
                    list[i].Id,
                    list[i].Name,
                    list[i].HomeSector,
                    list[i].HomeConstellationId,
                    list[i].Color,
                    list[i].FractionType);

            return result;
        }

        private static void EnsureIndex()
        {
            if (_byId != null)
                return;

            var all = GetAll();
            var dict = new Dictionary<int, Fraction>(all.Count);
            for (int i = 0; i < all.Count; i++)
                dict[all[i].Id] = all[i];

            _byId = dict;
        }
    }
}
