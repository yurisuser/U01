namespace _Project.DataAccess
{
    /// <summary>
    /// Простая структура строки локализации.
    /// </summary>
    public readonly struct LocalizationEntry
    {
        public LocalizationEntry(int id, string value)
        {
            Id = id;
            Value = value;
        }

        public int Id { get; }
        public string Value { get; }
    }
}
