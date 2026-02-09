using UnityEngine;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;
using _Project.Scripts.Ships;
using _Project.Scripts.Simulation.Ships;

namespace _Project.Scripts.Simulation.Local.Stages.Ai
{
    /// <summary>Создание NPC-корабля для тестового локального спавна.</summary>
    internal static class LocalSpawnShipFactory
    {
        public static Ship CreateShip()
        {
            var fractions = FractionService.GetAll();
            Fraction fraction;
            if (fractions == null || fractions.Count == 0)
            {
                fraction = new Fraction(0, "Default"); // Fallback, чтобы не блокировать спавн.
            }
            else
            {
                int attempts = 0;
                fraction = fractions[Random.Range(0, fractions.Count)];
                while (fraction.FractionType == EFractionTypes.Player && attempts < fractions.Count)
                {
                    int fracIndex = Random.Range(0, fractions.Count);
                    fraction = fractions[fracIndex];
                    attempts++; // Не даем бесконечно искать не-player фракцию.
                }
            }

            var pilotUid = UIDService.Create(EntityType.Individ); // Уникальный пилот для сущности ship.
            return ShipCreator.CreateShip(fraction, pilotUid);
        }
    }
}
