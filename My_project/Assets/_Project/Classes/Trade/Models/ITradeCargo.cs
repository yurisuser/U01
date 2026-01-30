using _Project.Items;

namespace _Project.Scripts.Trade.Models
{
    /// <summary>Единый доступ к карго для торговли.</summary>
    public interface ITradeCargo
    {
        int GetAmount(ItemType type, int itemId);
        bool CanAdd(ItemType type, int itemId, int amount);
        void Add(ItemType type, int itemId, int amount);
        void Remove(ItemType type, int itemId, int amount);
    }
}
