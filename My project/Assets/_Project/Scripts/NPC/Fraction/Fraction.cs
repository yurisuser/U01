namespace _Project.Scripts.NPC.Fraction
{
    public struct Fraction
    {
        public readonly EFraction Id;
        public readonly string Name;
        
        public Fraction(EFraction id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}