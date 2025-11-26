namespace _Project.Scripts.Galaxy.Data
{
    public struct PlanetSys
    {
        public Star MotherStar; // Материнская звезда
        public int OrbitIndex;    // Номер орбиты вокруг звезды
        public Planet Planet;
        public float OrbitPosition; //Угловая позиция на орбите
        public Moon[] Moons; //Массив спутников, если есть
    }
}