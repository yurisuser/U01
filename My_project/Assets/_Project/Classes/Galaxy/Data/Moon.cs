using _Project.Scripts.Core;

namespace _Project.Scripts.Galaxy.Data
{
    public struct Moon
    {
        public Core.UID Uid;
        public string Name;
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

        public string DisplayName => string.IsNullOrWhiteSpace(Name) ? string.Empty : Name;
    }
}
