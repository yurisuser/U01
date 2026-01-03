namespace _Project.Scripts.NPC.Fraction
{
    public struct Fraction
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int HomeSector;
        
        public Fraction(int id, string name, int homeSector = 0)
        {
            Id = id;
            Name = name;
            HomeSector = homeSector;
        }
    }
}
