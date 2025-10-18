using System;
using _Project.Scripts.NPC.Fraction;
using Random = UnityEngine.Random;

namespace _Project.Scripts.NPC.Individ
{
    public struct Individ
    {
        public readonly int Id;
        public readonly String Name;
        public readonly EFraction Frac;
        
        public readonly float Intellect;
        public readonly float Perception;
        public readonly float Willpower;
        public readonly float Physique;
       
        public ECurrentRole CurrentRole;            // текущая роль (Pilot/Marine)
        public int SystemId;           // Id системы

        public Individ(int id, string name, EFraction frac, int systemId)
        {
            Id = id;                    // фиксируем Id
            Name = name;                // фиксируем имя
            Frac = frac;                // фиксируем фракцию

            CurrentRole = ECurrentRole.Free;
            SystemId = systemId;
            Intellect = Random.Range(0.3f, 0.8f);
            Perception = Random.Range(0.3f, 0.8f);
            Willpower = Random.Range(0.3f, 0.8f);
            Physique = Random.Range(0.3f, 0.8f);
            
        }
    }
}