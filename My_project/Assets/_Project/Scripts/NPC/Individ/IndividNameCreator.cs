using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.NPC.Individ
{
    public static class IndividNameCreator
    {
        public static string Create(UID uid, EFraction fraction)
        {
            string prefix = fraction switch
            {
                EFraction.fraction1 => "Human",
                EFraction.fraction2 => "Hive",
                EFraction.fraction3 => "Machine",
                EFraction.fraction4 => "Nomad",
                EFraction.fraction5 => "Architect",
                EFraction.fraction6 => "Symbiont",
                _ => "Unknown"
            };
            return $"{prefix}#{uid:000000}";
        }
    }
}