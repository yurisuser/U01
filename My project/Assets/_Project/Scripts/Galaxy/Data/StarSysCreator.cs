namespace _Project.Scripts.Galaxy.Data
{
    /// <summary>
    /// Собирает StarSys на основе шаблона из спирали + финализированных планетных систем.
    /// ВАЖНО: мы копируем template, чтобы сохранить GalaxyPosition/OldX/OldY и т.п.
    /// </summary>
    public static class StarSysCreator
    {
        public static StarSys Create(StarSys template, Star star, int[] planetOrbits)
        {
            // финализируем планетные системы, собранные PlanetSysCreator'ом
            PlanetSys[] planetSystems = PlanetSysCreator.FinalizeAllOrbits(star);

            // копируем шаблон (struct-копия) и дополняем содержимое
            var sys = template;
            sys.Star         = star;           // поле из твоего StarSys
            sys.PlanetSysArr = planetSystems;  // поле из твоего StarSys (см. скрин)
            
            return sys;
        }
    }
}