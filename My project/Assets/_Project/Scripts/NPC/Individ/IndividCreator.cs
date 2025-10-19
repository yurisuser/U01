using _Project.Scripts.Core;
using _Project.Scripts.ID;
using _Project.Scripts.NPC.Fraction;
using Random = UnityEngine.Random;

namespace _Project.Scripts.NPC.Individ
{
    public static class IndividCreator
    {
        public static Individ Create(EFraction fraction, ECurrentRole role = ECurrentRole.Free)
        {
            UID id = IDService.Create(EntityType.Individ);                                      // уникальный идентификатор
            string name = IndividNameCreator.Create(id, fraction);    // имя на основе UID и фракции
            var s = GenerateInnateSkills();                           // базовые характеристики

            return new Individ(
                id,               // уникальный идентификатор
                name,             // имя индивида
                fraction,         // фракция
                role,             // текущая роль
                s.intellect,      // интеллект 0.0–1.0
                s.perception,     // восприятие 0.0–1.0
                s.willpower,      // сила воли 0.0–1.0
                s.physique        // физическая форма 0.0–1.0
            );
        }

        private static (float intellect, float perception, float willpower, float physique) GenerateInnateSkills()
        {
            float Range(float min, float max) => Random.value * (max - min) + min; // равномерный диапазон

            float intellect  = Range(0.3f, 0.8f);  // умственные способности
            float perception = Range(0.3f, 0.8f);  // восприятие
            float willpower  = Range(0.3f, 0.8f);  // сила воли
            float physique   = Range(0.3f, 0.8f);  // физическая форма

            return (intellect, perception, willpower, physique);
        }
    }
}