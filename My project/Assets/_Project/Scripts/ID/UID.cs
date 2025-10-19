using _Project.Scripts.Core;

namespace _Project.Scripts.ID
{
    // ReSharper disable once InconsistentNaming
    public struct UID
    {
        public readonly EntityType Type;
        public readonly int Id;
        
        public UID(EntityType type, int id)
        {
            this.Type = type;
            this.Id = id;
        }
    }
}