using _Project.Items;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Trade
{
    /// <summary>Участник сделки: владелец + доступ к карго.</summary>
    public interface ITradeActor
    {
        Fraction Owner { get; }
        Cargo Cargo { get; }
        long Money { get; }
    }
}
