namespace _Project.DataAccess
{
    public readonly struct CatalogConstellationName
    {
        public CatalogConstellationName(int id, string text)
        {
            Id = id;
            Text = text;
        }

        public int Id { get; }
        public string Text { get; }
    }
}
