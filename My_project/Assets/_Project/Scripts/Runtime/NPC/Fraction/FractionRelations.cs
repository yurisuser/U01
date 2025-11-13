using System;

namespace _Project.Scripts.NPC.Fraction
{
    /// <summary>
    /// Хранит статическую матрицу отношений между фракциями и отвечает на вопрос «кто для кого враг».
    /// </summary>
    public static class FractionRelations
    {
        private const int FractionCount = (int)EFraction.fraction25 + 1; // поддерживаем весь диапазон перечисления
        private static readonly bool[,] HostilityMatrix;

        static FractionRelations()
        {
            HostilityMatrix = new bool[FractionCount, FractionCount];
            InitializeDefaults();
        }

        /// <summary>
        /// Возвращает true, если цель считается враждебной для источника.
        /// </summary>
        public static bool IsHostile(EFraction source, EFraction target)
        {
            return HostilityMatrix[(int)source, (int)target];
        }

        /// <summary>
        /// Позволяет переопределить отношения в рантайме (например, при дипломатии).
        /// </summary>
        public static void SetHostility(EFraction source, EFraction target, bool hostile, bool mirror = true)
        {
            HostilityMatrix[(int)source, (int)target] = hostile;

            if (mirror)
                HostilityMatrix[(int)target, (int)source] = hostile;
        }

        private static void InitializeDefaults()
        {
            for (int i = 0; i < FractionCount; i++)
            {
                for (int j = 0; j < FractionCount; j++)
                {
                    HostilityMatrix[i, j] = i != j; // базово: своя фракция дружелюбна, остальные считаются врагами
                }
            }

            // Пример точечных настроек на будущее можно разместить здесь, например:
            // SetHostility(EFraction.fraction1, EFraction.fraction5, hostile: false); // люди и архитекторы нейтральны
        }
    }
}
