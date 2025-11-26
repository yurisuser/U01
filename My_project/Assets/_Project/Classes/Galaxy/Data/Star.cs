using _Project.Scripts.Core;

namespace _Project.Scripts.Galaxy.Data
{
    [System.Serializable]
    public struct Star
    {
        public UID Uid;
        public int NameId;
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

        public string Name
        {
            get
            {
                if (NameId < 0)
                    return string.Empty;

                return LocalizationDatabase.TryGetStarName(NameId, OldX, OldY, out var value)
                    ? value
                    : string.Empty;
            }
        }
    }
}
