using System;
using _Project.Scripts.Core;
using _Project.Scripts.NPC.Fraction;

namespace _Project.Scripts.NPC.Individ
{
    public struct Individ
    {
        public readonly UID Id;
        public readonly String Name;
        public readonly EFraction Frac;
        
        public readonly float Intellect;
        public readonly float Perception;
        public readonly float Willpower;
        public readonly float Physique;
       
        public ECurrentRole CurrentRole;            // текущая роль (Pilot/Marine)

        public Individ(
            UID id,              // уникальный идентификатор
            string name,          // имя индивида
            EFraction frac,       // фракция
            ECurrentRole role,    // текущая роль
            float intellect,      // интеллект 0.0–1.0
            float perception,     // восприятие 0.0–1.0
            float willpower,      // сила воли 0.0–1.0
            float physique        // физическая форма 0.0–1.0
        )
        {
            Id = id;                    // фиксируем UID
            Name = name;                // фиксируем имя
            Frac = frac;                // фиксируем фракцию
            CurrentRole = role;         // задаём текущую роль

            Intellect = intellect;      // задаём интеллект
            Perception = perception;    // задаём восприятие
            Willpower = willpower;      // задаём силу воли
            Physique = physique;        // задаём физическую форму
        }

    }
}