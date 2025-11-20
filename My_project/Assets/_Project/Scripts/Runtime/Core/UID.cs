namespace _Project.Scripts.Core
{
    /// <summary>
    /// Globally unique identifier: тип сущности и числовой Id.
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public struct UID
    {
        public readonly EntityType Type; ///<summary>Категория сущности.</summary>
        public readonly int Id; ///<summary>Числовой идентификатор внутри категории.</summary>
        
        /// <summary>
        /// Создаёт UID по типу сущности и числовому идентификатору.
        /// </summary>
        public UID(EntityType type, int id)
        {
            this.Type = type;
            this.Id = id;
        }
    }
}
