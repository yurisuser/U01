using _Project.Items;

namespace _Project.Industry.Recipes
{
    /// <summary>Статическое описание преобразования предметов за заданное число ходов.</summary>
    public sealed class Recipe
    {
        public string Key;                // текстовый ключ рецепта
        public int FactionId;             // автор рецепта
        public int Id;                   // идентификатор рецепта в каталоге
        public string Name;               // отображаемое имя рецепта
        public string Description;        // описание для интерфейса
        public ERecipeType Type;          // технологический тип рецепта
        public int CycleTurns;            // длительность одного производственного цикла в ходах
        public ItemStack[] Inputs;        // предметы, списываемые за цикл
        public ItemStack[] Outputs;       // предметы, создаваемые за цикл
    }
}
