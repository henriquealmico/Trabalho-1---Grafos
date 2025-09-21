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

        // --- Métricas de Grau ---
        private List<int> CalculateAllDegrees()
        {
            if (_allDegreesCache == null)
            {
                _allDegreesCache = new List<int>(VertexCount);
                for (int i = 1; i <= VertexCount; i++)
                {
                    _allDegreesCache.Add(_representation.GetNeighbors(i).Count());
                }
            }
            return _allDegreesCache;
        }

        public Dictionary<string, double> GetDegreeMetrics()
        {
            var degrees = CalculateAllDegrees();
            if (!degrees.Any()) return new Dictionary<string, double>
            {
                { "min_degree", 0 }, { "max_degree", 0 }, { "avg_degree", 0 }, { "median_degree", 0 }
            };
            
            var sortedDegrees = degrees.OrderBy(d => d).ToList();
            double median;
            int n = sortedDegrees.Count;
            if (n % 2 == 1)
                median = sortedDegrees[n / 2];
            else
                median = (sortedDegrees[n / 2 - 1] + sortedDegrees[n / 2]) / 2.0;

            return new Dictionary<string, double>
            {
                { "min_degree", degrees.Min() },
                { "max_degree", degrees.Max() },
                { "avg_degree", degrees.Average() },
                { "median_degree", median }
            };
        }

        // --- Algoritmos de Busca ---
        public (Dictionary<int, int?> Parents, Dictionary<int, int> Levels) BreadthFirstSearch(int startVertex)
        {
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            var queue = new Queue<int>();

            for (int i = 1; i <= VertexCount; i++) levels[i] = -1; // -1 significa não visitado

            levels[startVertex] = 0;
            parents[startVertex] = null; // A raiz não tem pai
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
            return (parents, levels);
        }

        public (Dictionary<int, int?> Parents, Dictionary<int, int> Levels) DepthFirstSearch(int startVertex)
        {
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
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
                
                foreach (var v in _representation.GetNeighbors(u).OrderByDescending(n => n))
                {
                    if (visited.Add(v))
                    {
                        parents[v] = u;
                        levels[v] = level + 1;
                        stack.Push((v, level + 1));
                    }
                }
            }
            return (parents, levels);
        }
        
        // --- Distância, Diâmetro e Componentes ---
        public int GetDistance(int u, int v)
        {
            var (_, levels) = BreadthFirstSearch(u);
            return levels.ContainsKey(v) ? levels[v] : -1;
        }

        public int GetDiameter()
        {
            if (VertexCount == 0) return 0;
            int maxDist = 0;
            for (int i = 1; i <= VertexCount; i++)
            {
                var (_, levels) = BreadthFirstSearch(i);
                int currentMax = levels.Values.Max();
                if (currentMax > maxDist)
                {
                    maxDist = currentMax;
                }
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
                if (!visited.Contains(i))
                {
                    var currentComponent = new List<int>();
                    var queue = new Queue<int>();
                    
                    queue.Enqueue(i);
                    visited.Add(i);
                    
                    while (queue.Count > 0)
                    {
                        int u = queue.Dequeue();
                        currentComponent.Add(u);
                        foreach (var v in _representation.GetNeighbors(u))
                        {
                            if (visited.Add(v))
                            {
                                queue.Enqueue(v);
                            }
                        }
                    }
                    components.Add(currentComponent);
                }
            }
            return components.OrderByDescending(c => c.Count).ToList();
        }

        // --- Método de Fábrica ---
        public static Graph FromFile(string filePath, Func<int, IGraphRepresentation> representationFactory)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
                throw new InvalidDataException("Arquivo de entrada está vazio.");

            if (!int.TryParse(lines[0], out int vertexCount))
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