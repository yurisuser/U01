using _Project.Scripts.Core;
namespace _Project.Scripts.Galaxy.Data
{
    [System.Serializable]
    public struct Star
    {
        public UID Uid;
        public string name;
        public EStarType type;
        public EStarSize size;
        public float temperature; // K
        public float mass;        // солнечные массы
        public float radius;      // солнечные радиусы
        public float luminosity;  // солнечные светимости
        public float age;              // млрд лет
        public float metallicity;      // 0–1
        public float stability;        // 0–1
    }
}