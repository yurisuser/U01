using _Project.Scripts.Core;

namespace _Project.Scripts.Galaxy.Data
{
    public struct Moon
    {
        public Core.UID Uid;
        public int NameId;            // ID записи в локализации
        public EMoonType Type;        // ��� ���
        public EMoonSize Size;        // ������
        public int OrbitIndex;        // ����� �ࡨ��
        public float Mass;            // ����
        public float Radius;          // ������
        public float OrbitDistance;   // �����ﭨ� �� �������
        public float OrbitPeriod;     // ��ਮ� ���饭�� 
        public float Inclination;     // ������ �ࡨ��
        public float Atmosphere;      // ���⭮��� �⬮����
        public float Temperature;     // �।��� ⥬������ �����孮��
        public float Gravity;         // �᪮७�� ᢮������� �������

        public string Name
        {
            get
            {
                if (NameId < 0)
                    return string.Empty;

                return LocalizationDatabase.TryGet(NameId, out var value)
                    ? value
                    : string.Empty;
            }
        }
    }
}
