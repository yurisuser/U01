namespace _Project.Scripts.Galaxy.Data
{
    public struct Planet
    {
        public Core.UID Uid;
        public int NameId;         // ID записи в локализации
        public float Mass;         // ���� (� �᫮���� ��.)
        public EPlanetType Type;   // ��� ������� (������ ������, ��������, ���ﭠ� � �.�.)
        public float Atmosphere;   // ���⭮��� �⬮���� (0 = ���, 1 = ������, >1 = ���⭥�)
        public float Radius;       // ������ (� ������ ࠤ���� ��� �᫮���� ��.)
        public float OrbitalDistance; // �����ﭨ� �� ������ (AU ��� �᫮��� ��.)
        public float OrbitalPeriod;   // ��ਮ� ���饭�� (� ������ ����� ��� �᫮���� ��.)
        public float Temperature;  // �।��� ⥬������ �����孮�� (K)
        public float Gravity;      // �ࠢ���� �� �����孮�� (g)
        public PlanetResource[] Resources;  // ����� ������ ����

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
