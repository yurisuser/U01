using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Const;
using _Project.Scripts.Galaxy.Data;
using Delaunator;
using UnityEngine;

namespace _Project.Scripts.Galaxy.Generation
{
    public static class ConstellationCreator
    {
        private static List<List<int>> _hypersList;
        private static Sector[] _sectorsArr;
        private static StarDistance[][] _distancesSorted;
        private static float[] _distanceFromCenter;
        private static StarSys[] _galaxy;
        private static int[] SectorsRows; //внешние радиусы окружностей, определяющие радиальные границы секторов
        private static float[][] SectorRowsSegments; //Границы сегментов в радианах для каждого слоя

        // Оркестратор: запускает стадии генерации и связывает их между собой
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

            BuildDistancesSorted();     // Предподготовка расстояний для быстрых выборок
            CreateHypers();             // Делоне-граф по позициям звёзд
            UnlinkPeriphery();          // Убираем связи у периферии
            //LinkPeriphery();            // Возвращаем по одной связи на периферию
            //InitConstellations();       // Сиды созвездий (по индексу)
            //----------------- Новый тип построения созвездий - по секторам
            InitSectorsRows();          //Расчет диапазонов секторов  
            InitRowsSegments();         //Расчет левой границы каждого сектора внутри сегмента
            ExpansionBySegment();       //Расширение созвездий по сегментам
            var constellationList = BuildConstellationList();
            GameBootstrap.GameState.SetConstellationList(constellationList);
            //-----------------
            //Expansion();                // Расширение созвездий по графу
            RemoveInterSectorConnection(); // Убираем межсозвездные связи и фиксируем лучшие мосты
            AddIntersectorConnection(); // Добавляем лучшие мосты между созвездиями
            SetMaxLinksLimit();         // Убираем лишние межзвездные связи
            LinkUnlinked();             // соединяем разорванные созвездия
            ApplyLinks();               // Запись линков в StarSys
        }

        public static HyperlinkEdge[] BuildHyperlinkEdges(StarSys[] galaxy)
        {
            if (galaxy == null || galaxy.Length == 0)
                return System.Array.Empty<HyperlinkEdge>();

            var edges = new List<HyperlinkEdge>();
            var edgeSet = new HashSet<long>();

            for (int i = 0; i < galaxy.Length; i++)
            {
                var links = galaxy[i].links;
                if (links == null || links.Length == 0)
                    continue;

                for (int j = 0; j < links.Length; j++)
                {
                    int other = links[j];
                    if (other < 0 || other >= galaxy.Length)
                        continue;

                    long key = GetEdgeKey(i, other);
                    if (!edgeSet.Add(key))
                        continue;

                    edges.Add(new HyperlinkEdge(i, other));
                }
            }

            return edges.ToArray();
        }

        // Доступ к рассчитанным границам для отладочного рендера.
        public static int[] GetSectorsRows() => SectorsRows;
        public static float[][] GetSectorRowsSegments() => SectorRowsSegments;

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
            // Полностью отрезаем центр и периферийные звёзды по OldX и радиусу.
            RemoveAllConnections(0); // Центральная чёрная дыра

            for (int i = 1; i < _galaxy.Length; i++)
            {
                if (_distanceFromCenter[i] > GalaxyConstants.PeripheryRadius) RemoveAllConnections(i);
            }
        }

        private static void LinkPeriphery()
        {
            // Для звёзд за радиусом возвращаем минимум одну связь, чтобы не остались изолированными.
            for (int i = 0; i < _distancesSorted[0].Length; i++)
            {
                if (_distancesSorted[0][i].distance < GalaxyConstants.PeripheryRadius) continue;
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
                if (_distanceFromCenter[idNeib] <= GalaxyConstants.PeripheryRadius)
                {
                    AddConnection(id, idNeib);
                    break;
                }
            }
        }

        private static void InitConstellations()
        {
            // Создаём созвездия по индексу: 1..ConstellationAmount-1.
            _sectorsArr = new Sector[GalaxyConstants.ConstellationAmount];
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

        private static void InitSectorsRows()
        {
            var weights = GalaxyConstants.ConstellationRows;
            if (weights == null || weights.Length == 0)
            {
                SectorsRows = System.Array.Empty<int>();
                return;
            }

            float totalWeight = 0f;
            for (int i = 0; i < weights.Length; i++)
                totalWeight += weights[i];

            if (totalWeight <= 0f)
            {
                SectorsRows = System.Array.Empty<int>();
                return;
            }

            float innerRadius = GalaxyConstants.MinStarInterval * GalaxyConstants.CentralBlackHoleIntervalK;
            float maxRadius = GalaxyConstants.PeripheryRadius;
            if (maxRadius < innerRadius)
                maxRadius = innerRadius;

            float range = maxRadius - innerRadius;
            SectorsRows = new int[weights.Length];

            float acc = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                acc += weights[i];
                float border = innerRadius + range * (acc / totalWeight);
                SectorsRows[i] = Mathf.RoundToInt(border);
            }
        }

        private static void InitRowsSegments()
        {
            var rows = GalaxyConstants.ConstellationRows;
            var sectors = GalaxyConstants.ConstellationSectors;
            var offsets = GalaxyConstants.ConstellationSectorsOffset;

            if (rows == null || sectors == null || offsets == null)
            {
                SectorRowsSegments = System.Array.Empty<float[]>();
                return;
            }

            if (rows.Length != sectors.Length || rows.Length != offsets.Length)
            {
                SectorRowsSegments = System.Array.Empty<float[]>();
                return;
            }

            SectorRowsSegments = new float[rows.Length][];
            float fullCircle = Mathf.PI * 2f;

            for (int i = 0; i < rows.Length; i++)
            {
                int segmentsCount = Mathf.Max(1, sectors[i]);
                float step = fullCircle / segmentsCount;
                // Смещение задано по часовой стрелке, поэтому инвертируем знак для CCW.
                float offsetCcw = -offsets[i];

                var borders = new float[segmentsCount];
                for (int s = 0; s < segmentsCount; s++)
                {
                    float angle = offsetCcw + step * s;
                    angle %= fullCircle;
                    if (angle < 0f)
                        angle += fullCircle;
                    borders[s] = angle;
                }

                System.Array.Sort(borders);
                SectorRowsSegments[i] = borders;
            }
        }

        private static void ExpansionBySegment()
        {
            if (_galaxy == null || _galaxy.Length == 0)
                return;
            if (SectorsRows == null || SectorRowsSegments == null)
                return;
            if (SectorsRows.Length == 0 || SectorRowsSegments.Length == 0)
                return;
            if (_hypersList == null)
                return;

            var sectorsPerRow = GalaxyConstants.ConstellationSectors;
            if (sectorsPerRow == null || sectorsPerRow.Length == 0)
                return;

            int rowsCount = Mathf.Min(SectorsRows.Length, SectorRowsSegments.Length);
            rowsCount = Mathf.Min(rowsCount, sectorsPerRow.Length);
            if (rowsCount <= 0)
                return;

            _sectorsArr = new Sector[GalaxyConstants.ConstellationAmount];
            var rowBaseId = new int[rowsCount];
            int baseId = 1;
            for (int r = 0; r < rowsCount; r++)
            {
                rowBaseId[r] = baseId;
                baseId += Mathf.Max(1, sectorsPerRow[r]);
            }

            float innerRadius = GalaxyConstants.MinStarInterval * GalaxyConstants.CentralBlackHoleIntervalK;
            float fullCircle = Mathf.PI * 2f;

            for (int i = 1; i < _galaxy.Length; i++)
            {
                if (_hypersList[i].Count == 0)
                    continue;

                var pos = _galaxy[i].GalaxyPosition;
                float radius = Mathf.Sqrt(pos.x * pos.x + pos.y * pos.y);
                if (radius < innerRadius)
                    continue;

                int rowIndex = rowsCount - 1;
                for (int r = 0; r < rowsCount; r++)
                {
                    if (radius <= SectorsRows[r])
                    {
                        rowIndex = r;
                        break;
                    }
                }

                var borders = SectorRowsSegments[rowIndex];
                if (borders == null || borders.Length == 0)
                    continue;

                float angle = Mathf.Atan2(pos.y, pos.x);
                if (angle < 0f)
                    angle += fullCircle;

                int segmentIndex = borders.Length - 1;
                if (angle < borders[0])
                {
                    segmentIndex = borders.Length - 1;
                }
                else
                {
                    for (int s = 1; s < borders.Length; s++)
                    {
                        if (angle < borders[s])
                        {
                            segmentIndex = s - 1;
                            break;
                        }
                    }
                }

                _galaxy[i].ConstellationId = rowBaseId[rowIndex] + segmentIndex;
                int cid = _galaxy[i].ConstellationId;
                if (cid > 0 && cid < _sectorsArr.Length)
                {
                    var sector = _sectorsArr[cid];
                    if (sector == null)
                    {
                        sector = new Sector
                        {
                            id = cid,
                            isOpen = true,
                            members = new List<MemberSector>()
                        };
                        _sectorsArr[cid] = sector;
                    }

                    sector.members.Add(new MemberSector
                    {
                        idSector = cid,
                        idSystem = i,
                        isOpen = false
                    });
                }
            }
        }

        private static int[][] BuildConstellationList()
        {
            if (_galaxy == null || _galaxy.Length == 0)
                return System.Array.Empty<int[]>();

            int maxId = 0;
            for (int i = 0; i < _galaxy.Length; i++)
            {
                int cid = _galaxy[i].ConstellationId;
                if (cid > maxId)
                    maxId = cid;
            }

            if (maxId <= 0)
                return System.Array.Empty<int[]>();

            var buckets = new List<int>[maxId + 1];
            for (int i = 0; i < _galaxy.Length; i++)
            {
                int cid = _galaxy[i].ConstellationId;
                if (cid <= 0)
                    continue;

                var list = buckets[cid];
                if (list == null)
                {
                    list = new List<int>();
                    buckets[cid] = list;
                }

                list.Add(i);
            }

            var result = new int[maxId + 1][];
            for (int i = 0; i < result.Length; i++)
                result[i] = buckets[i] == null ? System.Array.Empty<int>() : buckets[i].ToArray();

            return result;
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
                    var best = _sectorsArr[i].bestNeibourSectorMembersList[k];
                    int own = best.idOwnSys2 != 0 ? best.idOwnSys2 : best.idOwnSys;
                    int ext = best.idExternSys2 != 0 ? best.idExternSys2 : best.idExternSys;
                    AddConnection(own, ext);
                }
            }
        }

        private static void SetMaxLinksLimit()
        {
            int maxLinks = GalaxyConstants.MaxConstellationLinks;
            if (maxLinks <= 0 || _hypersList == null)
                return;

            bool changed;
            do
            {
                changed = false;
                for (int i = 0; i < _hypersList.Count; i++)
                {
                    while (_hypersList[i].Count > maxLinks)
                    {
                        int removeId = -1;
                        int bestDegree = -1;

                        for (int j = 0; j < _hypersList[i].Count; j++)
                        {
                            int neighborId = _hypersList[i][j];
                            if (_galaxy[i].ConstellationId != _galaxy[neighborId].ConstellationId) continue; // если созвездия разные - игнор
                            int degree = _hypersList[neighborId].Count;
                            if (degree > bestDegree)
                            {
                                bestDegree = degree;
                                removeId = neighborId;
                            }
                        }

                        if (removeId < 0) break;
                        if (_galaxy[i].ConstellationId != _galaxy[removeId].ConstellationId) break; // со второй стороны - если созвездия разные - игнор
                        RemoveConnection(i, removeId);
                        changed = true;
                    }
                }
            } while (changed);
        }

        private static void LinkUnlinked()
        {
            if (_galaxy == null || _hypersList == null)
                return;

            var byConstellation = new Dictionary<int, List<int>>();
            for (int i = 0; i < _galaxy.Length; i++)
            {
                int cid = _galaxy[i].ConstellationId;
                if (cid <= 0)
                    continue;

                if (!byConstellation.TryGetValue(cid, out var list))
                {
                    list = new List<int>();
                    byConstellation.Add(cid, list);
                }
                list.Add(i);
            }

            foreach (var entry in byConstellation)
            {
                int cid = entry.Key;
                var nodes = entry.Value;
                if (nodes.Count < 2)
                    continue;

                while (true)
                {
                    var components = new List<List<int>>();
                    var visited = new HashSet<int>();

                    for (int i = 0; i < nodes.Count; i++)
                    {
                        int start = nodes[i];
                        if (visited.Contains(start))
                            continue;

                        var comp = new List<int>();
                        var queue = new Queue<int>();
                        queue.Enqueue(start);
                        visited.Add(start);

                        while (queue.Count > 0)
                        {
                            int cur = queue.Dequeue();
                            comp.Add(cur);

                            var neigh = _hypersList[cur];
                            for (int n = 0; n < neigh.Count; n++)
                            {
                                int next = neigh[n];
                                if (_galaxy[next].ConstellationId != cid)
                                    continue;
                                if (visited.Add(next))
                                    queue.Enqueue(next);
                            }
                        }

                        components.Add(comp);
                    }

                    if (components.Count <= 1)
                        break;

                    float bestDist = float.MaxValue;
                    int bestA = -1;
                    int bestB = -1;

                    for (int a = 0; a < components.Count; a++)
                    {
                        for (int b = a + 1; b < components.Count; b++)
                        {
                            var compA = components[a];
                            var compB = components[b];

                            for (int i = 0; i < compA.Count; i++)
                            {
                                for (int j = 0; j < compB.Count; j++)
                                {
                                    float dist = Distance(compA[i], compB[j]);
                                    if (dist < bestDist)
                                    {
                                        bestDist = dist;
                                        bestA = compA[i];
                                        bestB = compB[j];
                                    }
                                }
                            }
                        }
                    }

                    if (bestA < 0 || bestB < 0)
                        break;

                    AddConnection(bestA, bestB);
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

        private static long GetEdgeKey(int a, int b)
        {
            int min = a < b ? a : b;
            int max = a < b ? b : a;
            return ((long)min << 32) | (uint)max;
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
                if (oldIndex < 0)
                {
                    bestNeibourSectorMembersList.Add(new BestNeibourSectorMember
                    {
                        idNeibourSector = _galaxy[idExtern].ConstellationId,
                        idOwnSys = idOwn,
                        idExternSys = idExtern,
                        distance = distance,
                        idOwnSys2 = 0,
                        idExternSys2 = 0,
                        distance2 = float.MaxValue
                    });
                    return;
                }
                var entry = bestNeibourSectorMembersList[oldIndex];
                if (entry.idOwnSys == idOwn && entry.idExternSys == idExtern)
                    return;
                if (entry.idOwnSys2 == idOwn && entry.idExternSys2 == idExtern)
                    return;

                if (distance < entry.distance)
                {
                    entry.idOwnSys2 = entry.idOwnSys;
                    entry.idExternSys2 = entry.idExternSys;
                    entry.distance2 = entry.distance;
                    entry.idOwnSys = idOwn;
                    entry.idExternSys = idExtern;
                    entry.distance = distance;
                }
                else if (distance < entry.distance2)
                {
                    entry.idOwnSys2 = idOwn;
                    entry.idExternSys2 = idExtern;
                    entry.distance2 = distance;
                }

                bestNeibourSectorMembersList[oldIndex] = entry;
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
            public int idOwnSys2;
            public int idExternSys2;
            public float distance2;
        }

        private struct StarDistance
        {
            public int index;
            public float distance;
        }
    }
}
