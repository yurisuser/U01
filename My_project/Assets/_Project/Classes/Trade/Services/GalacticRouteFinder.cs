using System.Collections.Generic;
using _Project.Scripts.Galaxy.Data;

namespace _Project.Scripts.Trade.Services
{
    /// <summary>Поиск кратчайшего числа прыжков между системами по HyperlinkEdge с кэшем.</summary>
    public static class GalacticRouteFinder
    {
        private static readonly Dictionary<(int, int), int> Cache = new();

        /// <summary>Возвращает число прыжков между системами, либо -1 если пути нет.</summary>
        public static int GetHops(int fromSystem, int toSystem, HyperlinkEdge[] edges, int systemsCount)
        {
            if (fromSystem == toSystem)
                return 0;
            if (fromSystem < 0 || toSystem < 0 || systemsCount <= 0)
                return -1;

            var key = NormalizeKey(fromSystem, toSystem);
            if (Cache.TryGetValue(key, out var cached))
                return cached;

            var adj = BuildAdjacency(edges, systemsCount);
            int hops = Bfs(fromSystem, toSystem, adj);
            Cache[key] = hops;
            return hops;
        }

        private static (int, int) NormalizeKey(int a, int b)
        {
            return a <= b ? (a, b) : (b, a);
        }

        private static List<int>[] BuildAdjacency(HyperlinkEdge[] edges, int systemsCount)
        {
            var list = new List<int>[systemsCount];
            for (int i = 0; i < systemsCount; i++)
                list[i] = new List<int>(4);

            if (edges != null)
            {
                for (int i = 0; i < edges.Length; i++)
                {
                    var e = edges[i];
                    if (e.A < 0 || e.A >= systemsCount || e.B < 0 || e.B >= systemsCount)
                        continue;
                    list[e.A].Add(e.B);
                    list[e.B].Add(e.A);
                }
            }

            return list;
        }

        private static int Bfs(int start, int target, List<int>[] adj)
        {
            var visited = new bool[adj.Length];
            var queue = new Queue<(int node, int dist)>();
            queue.Enqueue((start, 0));
            visited[start] = true;

            while (queue.Count > 0)
            {
                var (node, dist) = queue.Dequeue();
                var neighbors = adj[node];
                for (int i = 0; i < neighbors.Count; i++)
                {
                    int next = neighbors[i];
                    if (visited[next])
                        continue;
                    if (next == target)
                        return dist + 1;
                    visited[next] = true;
                    queue.Enqueue((next, dist + 1));
                }
            }

            return -1;
        }
    }
}
