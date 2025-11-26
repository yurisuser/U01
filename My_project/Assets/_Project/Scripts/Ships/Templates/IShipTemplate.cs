namespace _Project.Scripts.Ships
{
    public interface IShipTemplate // контракт для шаблонов кораблей (минимум данных)
    {
        ShipStats GetStats(); // вернуть базовые характеристики корабля
        byte GetWeaponSlots(); // вернуть число оружейных слотов (0..8)
    }
}

