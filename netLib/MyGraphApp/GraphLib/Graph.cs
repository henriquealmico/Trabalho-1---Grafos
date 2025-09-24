using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GraphLibrary.Representations;

namespace GraphLibrary
{
    public class Graph
    {
        private readonly IGraphRepresentation _representation;
        private List<int> _allDegreesCache;

        public int VertexCount => _representation.VertexCount;
        public int EdgeCount => _representation.EdgeCount;

        public Graph(int vertexCount, Func<int, IGraphRepresentation> representationFactory)
        {
            if (vertexCount <= 0)
                throw new ArgumentException("O número de vértices deve ser positivo.", nameof(vertexCount));
            
            _representation = representationFactory(vertexCount);
        }

        private void InvalidateDegreeCache() => _allDegreesCache = null;

        public void AddEdge(int u, int v)
        {
            if (u < 1 || u > VertexCount || v < 1 || v > VertexCount)
                throw new ArgumentOutOfRangeException($"Vértices devem estar no intervalo [1, {VertexCount}].");
            
            _representation.AddEdge(u, v);
            InvalidateDegreeCache();
        }

        public Dictionary<string, double> GetDegreeMetrics()
        {
            if (_allDegreesCache == null)
            {
                _allDegreesCache = new List<int>(VertexCount);
                for (int i = 1; i <= VertexCount; i++) _allDegreesCache.Add(_representation.GetNeighbors(i).Count());
            }
            var degrees = _allDegreesCache;

            if (!degrees.Any()) return new Dictionary<string, double> { { "min_degree", 0 }, { "max_degree", 0 }, { "avg_degree", 0 }, { "median_degree", 0 } };
            
            var sortedDegrees = degrees.OrderBy(d => d).ToList();
            double median = (sortedDegrees.Count % 2 == 1)
                ? sortedDegrees[sortedDegrees.Count / 2]
                : (sortedDegrees[sortedDegrees.Count / 2 - 1] + sortedDegrees[sortedDegrees.Count / 2]) / 2.0;

            return new Dictionary<string, double> { { "min_degree", degrees.Min() }, { "max_degree", degrees.Max() }, { "avg_degree", degrees.Average() }, { "median_degree", median } };
        }

        // --- OPTIMIZED SEARCH METHODS ---

        // Public version for convenience (allocates its own collections)
        public (Dictionary<int, int?> Parents, Dictionary<int, int> Levels) BreadthFirstSearch(int startVertex)
        {
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            BreadthFirstSearch(startVertex, parents, levels); // Call the high-performance version
            return (parents, levels);
        }

        // Internal version for high-performance loops (reuses collections)
        internal void BreadthFirstSearch(int startVertex, Dictionary<int, int?> parents, Dictionary<int, int> levels)
        {
            parents.Clear();
            levels.Clear();
            var queue = new Queue<int>();

            for (int i = 1; i <= VertexCount; i++) levels[i] = -1;

            levels[startVertex] = 0;
            parents[startVertex] = null;
            queue.Enqueue(startVertex);

            while (queue.Count > 0)
            {
                int u = queue.Dequeue();
                foreach (var v in _representation.GetNeighbors(u))
                {
                    if (levels[v] == -1)
                    {
                        levels[v] = levels[u] + 1;
                        parents[v] = u;
                        queue.Enqueue(v);
                    }
                }
            }
        }

        public (Dictionary<int, int?> Parents, Dictionary<int, int> Levels) DepthFirstSearch(int startVertex)
        {
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            DepthFirstSearch(startVertex, parents, levels);
            return (parents, levels);
        }

        internal void DepthFirstSearch(int startVertex, Dictionary<int, int?> parents, Dictionary<int, int> levels)
        {
            parents.Clear();
            levels.Clear();
            var visited = new HashSet<int>();
            var stack = new Stack<(int Vertex, int Level)>();

            for (int i = 1; i <= VertexCount; i++) levels[i] = -1;

            stack.Push((startVertex, 0));
            visited.Add(startVertex);
            parents[startVertex] = null;
            levels[startVertex] = 0;

            while (stack.Count > 0)
            {
                var (u, level) = stack.Pop();
                foreach (var v in _representation.GetNeighbors(u))
                {
                    if (visited.Add(v))
                    {
                        parents[v] = u;
                        levels[v] = level + 1;
                        stack.Push((v, level + 1));
                    }
                }
            }
        }

        public int GetDistance(int u, int v)
        {
            var (_, levels) = BreadthFirstSearch(u); // Uses the convenient public version
            return levels.GetValueOrDefault(v, -1);
        }

        public int GetDiameter()
        {
            if (VertexCount == 0) return 0;
            int maxDist = 0;
            // Reusing collections here as well for a significant boost
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            for (int i = 1; i <= VertexCount; i++)
            {
                BreadthFirstSearch(i, parents, levels);
                int currentMax = 0;
                foreach(var level in levels.Values) if(level > currentMax) currentMax = level;
                if (currentMax > maxDist) maxDist = currentMax;
            }
            return maxDist;
        }

        public int GetApproximateDiameter()
        {
            if (VertexCount == 0) return 0;
            var random = new Random();
            int s = random.Next(1, VertexCount + 1);
            var (_, levelsFromS) = BreadthFirstSearch(s);
            int u = levelsFromS.OrderByDescending(kvp => kvp.Value).First().Key;
            var (_, levelsFromU) = BreadthFirstSearch(u);
            return levelsFromU.Values.Max();
        }

        public List<List<int>> GetConnectedComponents()
        {
            var components = new List<List<int>>();
            var visited = new HashSet<int>();
            for (int i = 1; i <= VertexCount; i++)
            {
                if (visited.Add(i))
                {
                    var currentComponent = new List<int>();
                    var queue = new Queue<int>();
                    queue.Enqueue(i);
                    while (queue.Count > 0)
                    {
                        int u = queue.Dequeue();
                        currentComponent.Add(u);
                        foreach (var v in _representation.GetNeighbors(u)) if (visited.Add(v)) queue.Enqueue(v);
                    }
                    components.Add(currentComponent);
                }
            }
            return components.OrderByDescending(c => c.Count).ToList();
        }

        public static Graph FromFile(string filePath, Func<int, IGraphRepresentation> representationFactory)
        {
            var lines = File.ReadAllLines(filePath);
            if (!int.TryParse(lines.FirstOrDefault(), out int vertexCount))
                throw new InvalidDataException("A primeira linha deve conter o número de vértices.");

            var graph = new Graph(vertexCount, representationFactory);
            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out int u) && int.TryParse(parts[1], out int v))
                {
                    graph.AddEdge(u, v);
                }
            }
            return graph;
        }
    }
}