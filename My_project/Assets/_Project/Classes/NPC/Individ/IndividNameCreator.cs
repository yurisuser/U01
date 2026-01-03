using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.NPC.Individ
{
    public static class IndividNameCreator
    {
        public static string Create(UID uid, int fractionId)
        {
            string prefix = FractionService.TryGetNameById(fractionId, out var name)
                ? name
                : $"Faction{fractionId}";
            return $"{prefix}#{uid:000000}";
        }
    }
}
