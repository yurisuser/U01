using System;
using _Project.Scripts.Galaxy.Data;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class GalaxyCreator
    {
        // === ����ன�� ===
        private const int   StarCount                 = 1300;  // ������⢮ ��� � �����⨪�
        private const float GalaxyRadius              = 500f;  // ࠤ��� �����⨪� (��)
        private const float GalaxyStarLayer           = 0f;    // ᬥ饭�� ��� �� Z
        private const float DensityArms               = 3f;    // ���⭮��� ����� �㪠���
        private const float WidthArms                 = 60f;   // �ਭ� �㪠���
        private const float MinStarInterval           = 3.5f;  // �������쭮� ����ﭨ� ����� ��񧤠��
        private const float CentralBlackHoleIntervalK = 10f;   // ����� �� 業�� (��୮� ����)
        private const int   MaxAttemptsPerStar        = 64;    // ���ᨬ� ����⮪ ࠧ������ ������

        // ����७���
        private static float _lastRawX;
        private static float _lastRawY;

        public static StarSys[] Create()
        {
            var galaxy = CreateSpiralGalaxy(StarCount, GalaxyStarLayer); // ᮧ��� 蠡���
            LocalizationDatabase.PrepareStarNames(galaxy.Length);
            LocalizationDatabase.ResetDynamicValues();

            for (int i = 0; i < galaxy.Length; i++) // ��� ������ ������ �� ���� ����� ᫥���饥
            {
                ref var sysData = ref galaxy[i];
                sysData.NameId = i;

                if (i == 0)
                {
                    //� �⮩ �୥ �������� ��⮬
                    //
                    // ����ࠫ�� ��ꥪ�: ��ୠ� ���, ��� ������
                    //var starBh = new Star { type = EStarType.Black, size = EStarSize.Supergiant }; // size �� �������
                    //int[] noPlanets = Array.Empty<int>();
                    //galaxy[i] = StarSysCreator.Create(galaxy[i], starBh, noPlanets);
                    var coreStar = sysData.Star;
                    coreStar.NameId = i;
                    coreStar.OldX = sysData.OldX;
                    coreStar.OldY = sysData.OldY;
                    sysData.Star = coreStar;
                    continue;
                }

                var star = StarCreator.Create(); //ᮧ��� ������
                star.NameId = i;
                star.OldX = sysData.OldX;
                star.OldY = sysData.OldY;

                var planetOrbits = PlanetOrbitCreator.Create(star); //�ᯮ��㥬� �����⠬� �ࡨ��
                var planetsArr = new Planet[planetOrbits.Length];   //�� ������ �ࡨ� �� ������
                var planetSysArr = new PlanetSys[planetOrbits.Length]; //� �� ⮫쪮, � 楫�� �����⭠� ��⥬�
                var starDisplayName = LocalizationDatabase.GetStarName(star.NameId, star.OldX, star.OldY);

                for (var j = 0; j < planetOrbits.Length; j++) //��ॡ�ࠥ� �ࡨ�� � ������묨 ��⥬���
                {
                    var planet = PlanetCreator.Create(planetOrbits[j], star); //��� ������ �����⭮� ��⥬� ����� �������
                    var moonOrbits = MoonOrbitCreator.Create(planet);     // �ਪ��뢠� ᪮�쪮 �㤥� �ࡨ� ��� ��
                    var moonsArr = new Moon[moonOrbits.Length];                  // ����� �⮫쪮 �� � ��

                    for (var k = 0; k < moonOrbits.Length; k++) //��� ������ �㭭�� �ࡨ��
                    {
                        var moon = MoonCreator.Create(star, planetOrbits[j], planet, moonOrbits[k]); //ᮧ��� ���
                        if (!string.IsNullOrWhiteSpace(starDisplayName))
                        {
                            var moonName = LocalizationDatabase.ComposeMoonName(starDisplayName, j, k);
                            var moonId = LocalizationDatabase.RegisterDynamicValue(moonName);
                            if (moonId != int.MinValue)
                                moon.NameId = moonId;
                        }
                        moonsArr[k] = moon;
                    }

                    if (!string.IsNullOrWhiteSpace(starDisplayName))
                    {
                        var planetName = LocalizationDatabase.ComposePlanetName(starDisplayName, j);
                        var planetId = LocalizationDatabase.RegisterDynamicValue(planetName);
                        if (planetId != int.MinValue)
                            planet.NameId = planetId;
                    }

                    planetsArr[j] = planet;

                    planetSysArr[j] = PlanetSysCreator.Create(star, planetOrbits[j], planet, moonOrbits, moonsArr); //� �� �� ����稫��� - � �������� ��⥬�.
                }

                sysData = StarSysCreator.Create(sysData, star, planetSysArr, planetOrbits); //������� ��⥬� � ������ 㯠���뢠� � �������� ��⥬�
            }

            return galaxy;
        }

        private static StarSys[] CreateSpiralGalaxy(int count, float zLayer)
        {
            if (count <= 0) return Array.Empty<StarSys>();
            var arr = new StarSys[count];
            // 業��
            arr[0] = new StarSys //����ࠫ쭠� �ୠ� ���
            {
                GalaxyPosition = new Vector3(0f, 0f, zLayer),
                OldX = 0f,
                OldY = 0f
            };
            //��㣨� ��⥬�
            for (int i = 1; i < count; i++)
            {
                var sys = new StarSys();

                Vector3 pos = PlaceWithMinDistance(
                    index: i,
                    placed: arr,
                    sampleFunc: () => GenerateStarsNoGaussianDistr(zLayer),
                    baseMinDist: MinStarInterval,
                    centerExtraK: CentralBlackHoleIntervalK,
                    maxAttempts: MaxAttemptsPerStar
                );

                sys.GalaxyPosition = pos;
                sys.OldX = _lastRawX;
                sys.OldY = _lastRawY;

                arr[i] = sys;
            }

            return arr;
        }

        private static Vector3 GenerateStarsNoGaussianDistr(float zLayer)
        {
            // ᥬ� X � [-1;1], �������� �筮�� ��� (��� Atan(y/x))
            float xSeed = UnityEngine.Random.Range(-1f, 1f);
            if (Mathf.Approximately(xSeed, 0f)) xSeed = 0.0001f;

            float y = UnityEngine.Random.Range(-GalaxyRadius, GalaxyRadius);

            // ��������: �⥯��� ���� �� |xSeed|, ���� ����� ����� - ⠪ �� �㤥� NaN �� ��楫�� �⥯���
            float xCore = Mathf.Pow(Mathf.Abs(xSeed), DensityArms) * GalaxyRadius
                        + UnityEngine.Random.Range(-WidthArms, WidthArms);

            float sign = UnityEngine.Random.value > 0.5f ? 1f : -1f;
            float x = xCore * sign;

            _lastRawX = x;
            _lastRawY = y;

            return TwistCoordinates(new Vector3(x, y, zLayer));
        }

        // - ��� �뫮: Atan(y/x) + ᬥ�� ����� ࠤ��� ��� ��ண� �㪠�� -
        private static Vector3 TwistCoordinates(Vector3 vec)
        {
            // ���頥��� �� ������� �� ���� �����쪨� ��ᨫ���� � ������ X
            float xSafe = Mathf.Abs(vec.x) < 1e-4f ? (vec.x >= 0f ? 1e-4f : -1e-4f) : vec.x;

            float angle  = Mathf.Atan(vec.y / xSafe); // ��� � ��஬ 䠩��
            float radius = Mathf.Sqrt(vec.x * vec.x + vec.y * vec.y);

            // ��� ��� � ���� ��ன �㪠�
            radius *= (vec.x - vec.y) < 0f ? -1f : 1f;

            float normalizedRadius = Mathf.Abs(radius) / GalaxyRadius;
            float newAngle = angle + normalizedRadius * normalizedRadius * 4f;

            float x = Mathf.Cos(newAngle) * radius;
            float y = Mathf.Sin(newAngle) * radius;

            return new Vector3(x, y, GalaxyStarLayer);
        }

        private static Vector3 PlaceWithMinDistance(int index, StarSys[] placed, Func<Vector3> sampleFunc, float baseMinDist, float centerExtraK, int maxAttempts)
        {
            Vector3 lastSample = default;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                var candidate = sampleFunc();
                lastSample = candidate;

                // 䨫��� �� ��砩 �����-� ������᪨� ���祭��
                if (!IsFinite(candidate.x) || !IsFinite(candidate.y)) continue;

                bool ok = true;
                for (int j = 0; j < index; j++)
                {
                    float k = (j == 0) ? centerExtraK : 1f;
                    float minDist = baseMinDist * k;

                    if (Vector3.Distance(candidate, placed[j].GalaxyPosition) < minDist)
                    {
                        ok = false;
                        break;
                    }
                }

                if (ok) return candidate;
            }

            return lastSample;
        }

        private static bool IsFinite(float v) => !(float.IsNaN(v) || float.IsInfinity(v));
    }
}
