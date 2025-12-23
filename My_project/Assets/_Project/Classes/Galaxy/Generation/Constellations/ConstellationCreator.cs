using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;
using Delaunator;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class ConstellationCreator
    {
        // Константы как в олде (Settings.Galaxy)
        private const int ConstellationAmount = 20;
        private const float PeripheryRadius = 500f;

        private static List<List<int>> _hypersList;
        private static Sector[] _sectorsArr;
        private static StarDistance[][] _distancesSorted;
        private static float[] _distanceFromCenter;
        private static StarSys[] _galaxy;

        // Оркестратор: запускает стадии генерации и связывает их между собой.
        public static void Generate(StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return;

            _galaxy = galaxy;
            for (int i = 0; i < galaxy.Length; i++)
            {
                ref var sys = ref galaxy[i];
                sys.links = System.Array.Empty<int>();
                sys.ConstellationId = 0;
            }

            BuildDistancesSorted();   // Предподготовка расстояний для быстрых выборок
            CreateHypers();           // Делоне-граф по позициям звёзд
            UnlinkPeriphery();        // Убираем связи у периферии
            LinkPeriphery();          // Возвращаем по одной связи на периферию
            InitSectors();            // Сиды созвездий (по индексу)
            Expansion();              // Расширение созвездий по графу
            RemoveInterSectorConnection(); // Убираем межсозвездные связи и фиксируем лучшие мосты
            AddIntersectorConnection(); // Добавляем лучшие мосты между созвездиями
            ApplyLinks();               // Запись линков в StarSys
        }

        private static void BuildDistancesSorted()
        {
            // Для каждой звезды строим список всех расстояний до остальных, сортируем по близости.
            // Используется для быстрых выборок ближайших систем и радиуса от центра.
            int count = _galaxy.Length;
            _distancesSorted = new StarDistance[count][];
            _distanceFromCenter = new float[count];

            for (int i = 0; i < count; i++)
            {
                var list = new StarDistance[count];
                for (int j = 0; j < count; j++)
                {
                    float dist = Distance(i, j);
                    list[j] = new StarDistance
                    {
                        index = j,
                        distance = dist
                    };
                }

                System.Array.Sort(list, (a, b) => a.distance >= b.distance ? 1 : -1);
                _distancesSorted[i] = list;
            }

            for (int i = 0; i < count; i++)
            {
                var entry = _distancesSorted[0][i];
                _distanceFromCenter[entry.index] = entry.distance; // Дистанция от центра (индекс 0)
            }
        }

        private static float Distance(int a, int b)
        {
            var aPos = _galaxy[a].GalaxyPosition;
            var bPos = _galaxy[b].GalaxyPosition;
            return Vector3.Distance(aPos, bPos);
        }

        private static void CreateHypers()
        {
            // Строим делоне-триангуляцию по XY координатам и превращаем треугольники в граф связей.
            var coords = new List<double>(_galaxy.Length * 2);
            for (int i = 0; i < _galaxy.Length; i++)
            {
                var pos = _galaxy[i].GalaxyPosition;
                coords.Add(pos.x);
                coords.Add(pos.y);
            }

            var tr = new Triangulation(coords);

            _hypersList = new List<List<int>>(_galaxy.Length);
            for (int i = 0; i < _galaxy.Length; i++)
                _hypersList.Add(new List<int>());

            for (int i = 0; i < tr.triangles.Count - 2; i += 3)
            {
                AddConnection(tr.triangles[i], tr.triangles[i + 1]);
                AddConnection(tr.triangles[i + 1], tr.triangles[i + 2]);
                AddConnection(tr.triangles[i + 2], tr.triangles[i]);
            }
        }

        private static void UnlinkPeriphery()
        {
            // Полностью отрезаем центр и периферийные звёзды за радиусом.
            RemoveAllConnections(0); // Центральная чёрная дыра

            for (int i = 1; i < _galaxy.Length; i++)
            {
                if (_distanceFromCenter[i] > PeripheryRadius)
                    RemoveAllConnections(i);
            }
        }

        private static void LinkPeriphery()
        {
            // Для звёзд за радиусом возвращаем минимум одну связь, чтобы не остались изолированными.
            for (int i = 0; i < _distancesSorted[0].Length; i++)
            {
                if (_distancesSorted[0][i].distance < PeripheryRadius)
                    continue;
                AddOnceForClear(_distancesSorted[0][i].index);
            }
        }

        private static void AddOnceForClear(int id)
        {
            // Возвращаем одну связь, чтобы периферия не была изолированной
            for (int i = 1; i < _distancesSorted[id].Length; i++)
            {
                int idNeib = _distancesSorted[id][i].index;
                if (_hypersList[idNeib].Count > 0)
                {
                    AddConnection(id, idNeib);
                    break;
                }
                if (_distanceFromCenter[idNeib] <= PeripheryRadius)
                {
                    AddConnection(id, idNeib);
                    break;
                }
            }
        }

        private static void InitSectors()
        {
            // Создаём созвездия по индексу: 1..ConstellationAmount-1.
            _sectorsArr = new Sector[ConstellationAmount];
            for (int i = 1; i < _sectorsArr.Length; i++)
            {
                if (i >= _galaxy.Length)
                    break;

                var sector = new Sector
                {
                    id = i,
                    isOpen = true,
                    members = new List<MemberSector>()
                };
                var member = new MemberSector
                {
                    idSector = i,
                    idSystem = i,
                    isOpen = true
                };
                _galaxy[member.idSystem].ConstellationId = member.idSector;
                sector.members.Add(member);
                _sectorsArr[i] = sector;
            }
        }

        private static void Expansion()
        {
            // По очереди расширяем все созвездия, пока есть куда расти.
            bool key = true;
            while (key)
            {
                key = false;
                for (int i = 1; i < _sectorsArr.Length; i++)
                {
                    if (_sectorsArr[i] == null || !_sectorsArr[i].isOpen)
                        continue;
                    ExpansionSector(_sectorsArr[i]);
                    key = true;
                }
            }
        }

        private static void ExpansionSector(Sector sector)
        {
            // Пытаемся расширить сектор через первого открытого участника.
            for (int i = 0; i < sector.members.Count; i++)
            {
                if (!sector.members[i].isOpen)
                    continue;
                int idNewMember = GetNewSystem(sector.members[i]);
                if (idNewMember == 0)
                {
                    sector.members[i].isOpen = false;
                    continue;
                }
                sector.members.Add(CreateNewMember(idNewMember, sector.id));
                return;
            }
            sector.isOpen = false;
        }

        private static int GetNewSystem(MemberSector member)
        {
            // Берём первого соседа по графу, который ещё не принадлежит созвездию.
            for (int i = 0; i < _hypersList[member.idSystem].Count; i++)
            {
                int neibourId = _hypersList[member.idSystem][i];
                if (_galaxy[neibourId].ConstellationId == 0)
                    return neibourId;
            }
            return 0;
        }

        private static MemberSector CreateNewMember(int idSystem, int idSector)
        {
            _galaxy[idSystem].ConstellationId = idSector;
            var member = new MemberSector
            {
                idSector = idSector,
                idSystem = idSystem,
                isOpen = true
            };
            return member;
        }

        private static void AddConnection(int idA, int idB)
        {
            if (!_hypersList[idA].Contains(idB))
                _hypersList[idA].Add(idB);
            if (!_hypersList[idB].Contains(idA))
                _hypersList[idB].Add(idA);
        }

        private static void AddIntersectorConnection()
        {
            for (int i = 1; i < _sectorsArr.Length; i++)
            {
                if (_sectorsArr[i] == null)
                    continue;

                for (int k = 0; k < _sectorsArr[i].bestNeibourSectorMembersList.Count; k++)
                {
                    AddConnection(
                        _sectorsArr[i].bestNeibourSectorMembersList[k].idOwnSys,
                        _sectorsArr[i].bestNeibourSectorMembersList[k].idExternSys
                    );
                }
            }
        }

        private static void RemoveConnection(int idA, int idB)
        {
            _hypersList[idA].Remove(idB);
            _hypersList[idB].Remove(idA);
        }

        private static void RemoveAllConnections(int id)
        {
            for (int i = 0; i < _hypersList[id].Count; i++)
            {
                var idNear = _hypersList[id][i];
                _hypersList[idNear].Remove(id);
            }
            _hypersList[id].Clear();
        }

        private static void RemoveInterSectorConnection()
        {
            // Удаляем все межсозвездные связи, сохраняя лучший мост между парами созвездий.
            for (int i = 0; i < _hypersList.Count; i++)
            {
                for (int k = _hypersList[i].Count - 1; k >= 0; k--)
                {
                    int targetId = _hypersList[i][k];
                    if (_galaxy[i].ConstellationId != _galaxy[targetId].ConstellationId)
                    {
                        SaveBestConnection(i, targetId);
                        RemoveConnection(i, targetId);
                    }
                }
                if (_galaxy[i].ConstellationId == 0)
                    RemoveAllConnections(i); // Системы вне созвездий не оставляем
            }
        }

        private static void SaveBestConnection(int idSysA, int idSysB)
        {
            // Сохраняем лучший мост по расстоянию между парой созвездий.
            if (_galaxy[idSysA].ConstellationId <= 0 || _galaxy[idSysB].ConstellationId <= 0)
                return;

            var secA = _sectorsArr[_galaxy[idSysA].ConstellationId];
            var secB = _sectorsArr[_galaxy[idSysB].ConstellationId];
            if (secA == null || secB == null)
                return;

            secA.AddBest(idSysA, idSysB, Distance(idSysA, idSysB));
            secB.AddBest(idSysB, idSysA, Distance(idSysA, idSysB));
        }

        private static void ApplyLinks()
        {
            // Записываем рассчитанные связи в StarSys.
            for (int i = 0; i < _hypersList.Count; i++)
            {
                ref var sys = ref _galaxy[i];
                sys.links = _hypersList[i].ToArray();
            }
        }

        private class Sector
        {
            public int id;
            public List<MemberSector> members;
            public bool isOpen;
            public List<BestNeibourSectorMember> bestNeibourSectorMembersList = new List<BestNeibourSectorMember>();

            public void AddBest(int idOwn, int idExtern, float distance)
            {
                int oldIndex = bestNeibourSectorMembersList.FindIndex(x => x.idNeibourSector == _galaxy[idExtern].ConstellationId);
                var addingBest = new BestNeibourSectorMember
                {
                    idNeibourSector = _galaxy[idExtern].ConstellationId,
                    idOwnSys = idOwn,
                    idExternSys = idExtern,
                    distance = distance
                };
                if (oldIndex < 0)
                {
                    bestNeibourSectorMembersList.Add(addingBest);
                    return;
                }
                if (bestNeibourSectorMembersList[oldIndex].distance > distance)
                {
                    bestNeibourSectorMembersList[oldIndex] = addingBest;
                }
            }
        }

        private class MemberSector
        {
            public int idSystem;
            public int idSector;
            public bool isOpen;
        }

        private struct BestNeibourSectorMember
        {
            public int idNeibourSector;
            public int idOwnSys;
            public int idExternSys;
            public float distance;
        }

        private struct StarDistance
        {
            public int index;
            public float distance;
        }
    }
}
