using _Project.CONST;

namespace _Project.Scripts.NPC.Fraction
{
    public class Fraction
    {
        public readonly int Id;
        public readonly string Name;
        public readonly int HomeSector;
        public readonly int HomeConstellationId;
        public readonly string Color;
        public readonly EFractionTypes FractionType;

        private long _money;

        public long Money
        {
            get => FractionType == EFractionTypes.Player ? _money : EconomyConstants.NpcInfiniteMoney; // деньги игрока реальные, у NPC фиктивный лимит
            set
            {
                if (FractionType == EFractionTypes.Player) // менять деньги можно только у игрока
                    _money = value;
            }
        }
        
        public Fraction(int id, string name, int homeSector = 0, int homeConstellationId = 0, 
        string color = null, EFractionTypes fractionType = EFractionTypes.Regular, long money = 0)
        {
            Id = id;
            Name = name;
            HomeSector = homeSector;
            HomeConstellationId = homeConstellationId;
            Color = color;
            FractionType = fractionType;
            _money = money;
        }
    }
}
