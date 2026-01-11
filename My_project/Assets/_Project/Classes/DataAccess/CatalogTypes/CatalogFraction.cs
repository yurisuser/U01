using System.Collections.Generic;

namespace _Project.DataAccess
{
    public readonly struct CatalogFraction
    {
        public CatalogFraction(
            int id,
            string name,
            string bio,
            string politic,
            string color,
            int homeSector,
            int homeConstellationId,
            string symbol,
            string description,
            IReadOnlyList<string> starNames,
            IReadOnlyList<string> planetNames,
            IReadOnlyList<string> moonNames)
        {
            Id = id;
            Name = name;
            Bio = bio;
            Politic = politic;
            Color = color;
            HomeSector = homeSector;
            HomeConstellationId = homeConstellationId;
            Symbol = symbol;
            Description = description;
            StarNames = starNames;
            PlanetNames = planetNames;
            MoonNames = moonNames;
        }

        public int Id { get; }
        public string Name { get; }
        public string Bio { get; }
        public string Politic { get; }
        public string Color { get; }
        public int HomeSector { get; }
        public int HomeConstellationId { get; }
        public string Symbol { get; }
        public string Description { get; }
        public IReadOnlyList<string> StarNames { get; }
        public IReadOnlyList<string> PlanetNames { get; }
        public IReadOnlyList<string> MoonNames { get; }
    }
}
