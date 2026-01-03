namespace _Project.Scripts.NPC.Fraction
{
    public struct Fraction
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int HomeSector;
        public readonly string Color;
        
        public Fraction(int id, string name, int homeSector = 0, string color = null)
        {
            Id = id;
            Name = name;
            HomeSector = homeSector;
            Color = color;
        }
    }
}
