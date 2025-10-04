using Random = UnityEngine.Random;
using  CONST = _Project.CONSTANT.GALAXY;
namespace _Project.Scripts.Galaxy.Data
{
    public static class StarCreator
    {
        public static Star Create(StarType? forcedType = null, StarSize? forcedSize = null)
        {
            Star star = new Star();

            // Тип
            StarType type = forcedType ?? GetRandomStarType();
            star.type = type;

            // Размер
            StarSize size = forcedSize ?? GetRandomStarSize(type);
            star.size = size;

            // Физика
            ApplyPhysics(ref star);

            return star;
        }

        private static StarType GetRandomStarType()
        {
            int total = CONST.StarRedWeight + CONST.StarOrangeWeight + CONST.StarYelloWeight +
                        CONST.StarWhiteWeight + CONST.StarBlueWeight + CONST.StarNeutronWeight +
                        CONST.StarBlackWeight;

            int roll = Random.Range(0, total);
            if ((roll -= CONST.StarRedWeight) < 0) return StarType.Red;
            if ((roll -= CONST.StarOrangeWeight) < 0) return StarType.Orange;
            if ((roll -= CONST.StarYelloWeight) < 0) return StarType.Yellow;
            if ((roll -= CONST.StarWhiteWeight) < 0) return StarType.White;
            if ((roll -= CONST.StarBlueWeight) < 0) return StarType.Blue;
            if ((roll -= CONST.StarNeutronWeight) < 0) return StarType.Neutron;
            return StarType.Black;
        }

        private static StarSize GetRandomStarSize(StarType type)
        {
            int roll, total;
            switch (type)
            {
                case StarType.Red:
                    total = CONST.RedDwarfWeight + CONST.RedNormalWeight + CONST.RedGiantWeight + CONST.RedSuperGiantWeight;
                    roll = Random.Range(0, total);
                    if ((roll -= CONST.RedDwarfWeight) < 0) return StarSize.Dwarf;
                    if ((roll -= CONST.RedNormalWeight) < 0) return StarSize.Normal;
                    if ((roll -= CONST.RedGiantWeight) < 0) return StarSize.Giant;
                    return StarSize.Supergiant;

                case StarType.Orange:
                case StarType.Yellow:
                case StarType.White:
                    total = CONST.SolarDwarfWeight + CONST.SolarNormalWeight + CONST.SolarGiantWeight + CONST.SolarSuperGiantWeight;
                    roll = Random.Range(0, total);
                    if ((roll -= CONST.SolarDwarfWeight) < 0) return StarSize.Dwarf;
                    if ((roll -= CONST.SolarNormalWeight) < 0) return StarSize.Normal;
                    if ((roll -= CONST.SolarGiantWeight) < 0) return StarSize.Giant;
                    return StarSize.Supergiant;

                case StarType.Blue:
                    total = CONST.BlueDwarfWeight + CONST.BlueNormalWeight + CONST.BlueGiantWeight + CONST.BlueSuperGiantWeight;
                    roll = Random.Range(0, total);
                    if ((roll -= CONST.BlueDwarfWeight) < 0) return StarSize.Dwarf;
                    if ((roll -= CONST.BlueNormalWeight) < 0) return StarSize.Normal;
                    if ((roll -= CONST.BlueGiantWeight) < 0) return StarSize.Giant;
                    return StarSize.Supergiant;

                case StarType.Neutron:
                    return StarSize.Dwarf; // фикс размер

                case StarType.Black:
                    return StarSize.Supergiant; // фикс размер
            }
            return StarSize.Normal;
        }

        private static void ApplyPhysics(ref Star star)
        {
            // Температура
            (float min, float max) tempRange = star.type switch
            {
                StarType.Red     => CONST.TempRed,
                StarType.Orange  => CONST.TempOrange,
                StarType.Yellow   => CONST.TempYello,
                StarType.White   => CONST.TempWhite,
                StarType.Blue    => CONST.TempBlue,
                StarType.Neutron => CONST.TempNeutron,
                StarType.Black   => CONST.TempBlack,
                _ => (0f, 0f)
            };
            star.temperature = Random.Range(tempRange.min, tempRange.max);

            // Масса / Радиус / Светимость
            (float mMin, float mMax, float rMin, float rMax, float lMin, float lMax) prof = star.size switch
            {
                StarSize.Dwarf      => CONST.ProfDwarf,
                StarSize.Normal     => CONST.ProfNormal,
                StarSize.Giant      => CONST.ProfGiant,
                StarSize.Supergiant => CONST.ProfSuper,
                _ => CONST.ProfNormal
            };
            star.mass       = Random.Range(prof.mMin, prof.mMax);
            star.radius     = Random.Range(prof.rMin, prof.rMax);
            star.luminosity = Random.Range(prof.lMin, prof.lMax);
            star.age         = Random.Range(0.1f, 13.5f); // 0.1–13.5 млрд лет
            star.metallicity = Random.Range(0f, 1f);      // от бедных до богатых металлами
            star.stability   = Random.Range(0f, 1f);      // условная стабильность
            star.habitability= Random.Range(0f, 1f);      // пригодность для жизни
        }
    }
}
