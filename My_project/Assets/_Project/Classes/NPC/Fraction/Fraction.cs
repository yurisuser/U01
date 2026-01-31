namespace _Project.Scripts.NPC.Fraction
{
    public struct Fraction
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int HomeSector;
        public readonly int HomeConstellationId;
        public readonly string Color;
        public readonly EFractionTypes FractionType;

        public long Money;
        
        public Fraction(int id, string name, int homeSector = 0, int homeConstellationId = 0, 
        string color = null, EFractionTypes fractionType = EFractionTypes.Regular, long money = 0)
        {
            Id = id;
            Name = name;
            HomeSector = homeSector;
            HomeConstellationId = homeConstellationId;
            Color = color;
            FractionType = fractionType;
            Money = money;
        }
    }
}
