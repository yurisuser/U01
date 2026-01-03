using System;
using _Project.Scripts.Core;

namespace _Project.Scripts.NPC.Individ
{
    public struct Individ
    {
        public readonly UID Id;
        public readonly String Name;
        public readonly int FractionId;        
        public readonly float Intellect;
        public readonly float Perception;
        public readonly float Willpower;
        public readonly float Physique;
       
        public ECurrentRole CurrentRole;            // текущая роль (Pilot/Marine)

        public Individ(
            UID id,              // уникальный идентификатор
            string name,          // имя индивида
            int fractionId,       // фракция
            ECurrentRole role,    // текущая роль
            float intellect,      // интеллект 0.0–1.0
            float perception,     // восприятие 0.0–1.0
            float willpower,      // сила воли 0.0–1.0
            float physique        // физическая форма 0.0–1.0
        )
        {
            Id = id;                    // фиксируем UID
            Name = name;                // фиксируем имя
            FractionId = fractionId;    // фиксируем фракцию
            CurrentRole = role;         // задаём текущую роль

            Intellect = intellect;      // задаём интеллект
            Perception = perception;    // задаём восприятие
            Willpower = willpower;      // задаём силу воли
            Physique = physique;        // задаём физическую форму
        }

    }
}
