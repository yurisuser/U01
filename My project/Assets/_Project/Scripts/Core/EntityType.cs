namespace _Project.Scripts.Core
{
    public enum EntityType
    {
        None = 0,     // отсутствует или не определён
        Individ,      // индивидуум (пилот, член экипажа и т.п.)
        Ship,         // корабль
        Fraction,     // фракция
        Star,
        System,       // звёздная система
        Planet,       // планета
        Moon,         // луна
        Station,      // космическая станция
        Event         // игровое событие
    }
}