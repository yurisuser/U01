using _Project.Scripts.Core;
using _Project.DataAccess;

namespace _Project.Scripts.Galaxy.Data
{
    [System.Serializable]
    public struct Star
    {
        public UID Uid;
        public string Name;
        public float OldX;
        public float OldY;
        public EStarType type;
        public EStarSize size;
        public float temperature; // K
        public float mass;        // ᮫���� �����
        public float radius;      // ᮫���� ࠤ����
        public float luminosity;  // ᮫���� ᢥ⨬���
        public float age;              // ��� ���
        public float metallicity;      // 0-1
        public float stability;        // 0-1
    }
}
