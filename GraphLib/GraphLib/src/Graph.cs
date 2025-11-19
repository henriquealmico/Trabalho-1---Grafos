using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GraphLibrary.Representations;
using GraphLibrary.Algorithms;

namespace GraphLibrary {
    public class Graph {
        private readonly IGraphRepresentation _representation;
        private readonly Func<int, IGraphRepresentation> _representationFactory;
        private readonly IDijkstraStrategy _dijkstraStrategy;
        private readonly bool _isDirected;
        private List<int> _allDegreesCache;
        
        private readonly Dictionary<string, int> _vertexStringToInt = new();
        private readonly Dictionary<int, string> _vertexIntToString = new();

        public int VertexCount => _representation.VertexCount;
        public int EdgeCount => _representation.EdgeCount;
        public bool HasNegativeWeights { get; private set; }
        public bool IsDirected => _isDirected;

        public Graph(int vertexCount,
                     Func<int, IGraphRepresentation> representationFactory,
                     IDijkstraStrategy dijkstraStrategy = null,
                     bool isDirected = false) {
            if (vertexCount <= 0)
                throw new ArgumentException("O número de vértices deve ser positivo.", nameof(vertexCount));
            
            _representationFactory = representationFactory ?? throw new ArgumentNullException(nameof(representationFactory));
            _representation = _representationFactory(vertexCount);
            _dijkstraStrategy = dijkstraStrategy ?? new DijkstraHeapStrategy();
            _isDirected = isDirected;
        }

        private void InvalidateDegreeCache() => _allDegreesCache = null;

        private void ValidateVertexIndex(int vertex) {
            if (vertex < 1 || vertex > VertexCount)
                throw new ArgumentOutOfRangeException($"Vértices devem estar no intervalo [1, {VertexCount}].");
        }

        public void AddEdge(int u, int v, double weight) {
            ValidateVertexIndex(u);
            ValidateVertexIndex(v);

            if (weight < 0) HasNegativeWeights = true;
            
            _representation.AddEdge(u, v, weight, _isDirected);
            InvalidateDegreeCache();
        }

        public void AddEdge(int u, int v) {
            AddEdge(u, v, 1.0);
        }

        public IEnumerable<(int neighbor, double weight)> GetNeighbors(int vertex) {
            ValidateVertexIndex(vertex);
            return _representation.GetNeighbors(vertex);
        }

        public (Dictionary<int, int?> Parents, Dictionary<int, int> Levels) BreadthFirstSearch(int startVertex) {
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            BreadthFirstSearch(startVertex, parents, levels);
            return (parents, levels);
        }
        
        internal void BreadthFirstSearch(int startVertex, Dictionary<int, int?> parents, Dictionary<int, int> levels) {
            parents.Clear();
            levels.Clear();
            var queue = new Queue<int>();

            for (var i = 1; i <= VertexCount; i++) levels[i] = -1;

            levels[startVertex] = 0;
            parents[startVertex] = null;
            queue.Enqueue(startVertex);

            while (queue.Count > 0) {
                var u = queue.Dequeue();
                foreach (var (neighbor, _) in _representation.GetNeighbors(u)) {
                    if (levels[neighbor] != -1) 
                        continue;
                    
                    levels[neighbor] = levels[u] + 1;
                    parents[neighbor] = u;
                    queue.Enqueue(neighbor);
                }
            }
        }
        
        public (Dictionary<int, int?> Parents, Dictionary<int, int> Levels) DepthFirstSearch(int startVertex) {
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            DepthFirstSearch(startVertex, parents, levels);
            return (parents, levels);
        }

        internal void DepthFirstSearch(int startVertex, Dictionary<int, int?> parents, Dictionary<int, int> levels) {
            parents.Clear();
            levels.Clear();
            var visited = new HashSet<int>();
            var stack = new Stack<(int Vertex, int Level)>();

            for (var i = 1; i <= VertexCount; i++) levels[i] = -1;

            stack.Push((startVertex, 0));
            visited.Add(startVertex);
            parents[startVertex] = null;
            levels[startVertex] = 0;

            while (stack.Count > 0) {
                var (u, level) = stack.Pop();
                foreach (var (neighbor, _) in _representation.GetNeighbors(u)) {
                    if (!visited.Add(neighbor)) 
                        continue;
                    
                    parents[neighbor] = u;
                    levels[neighbor] = level + 1;
                    stack.Push((neighbor, level + 1));
                }
            }
        }

        public int GetDistance(int u, int v) {
            var (_, levels) = BreadthFirstSearch(u);
            return levels.GetValueOrDefault(v, -1);
        }

        public int GetDiameter() {
            if (VertexCount == 0) return 0;
            var maxDist = 0;
            var parents = new Dictionary<int, int?>(VertexCount);
            var levels = new Dictionary<int, int>(VertexCount);
            for (var i = 1; i <= VertexCount; i++) {
                BreadthFirstSearch(i, parents, levels);
                var currentMax = 0;
                foreach(var level in levels.Values) if(level > currentMax) currentMax = level;
                if (currentMax > maxDist) maxDist = currentMax;
            }
            return maxDist;
        }

        public int GetApproximateDiameter() {
            if (VertexCount == 0) return 0;
            var random = new Random();
            var s = random.Next(1, VertexCount + 1);
            var (_, levelsFromS) = BreadthFirstSearch(s);
            var u = levelsFromS.OrderByDescending(kvp => kvp.Value).First().Key;
            var (_, levelsFromU) = BreadthFirstSearch(u);
            return levelsFromU.Values.Max();
        }

        public List<List<int>> GetConnectedComponents() {
            var components = new List<List<int>>();
            var visited = new HashSet<int>();
            for (var i = 1; i <= VertexCount; i++) {
                if (visited.Add(i)) {
                    var currentComponent = new List<int>();
                    var queue = new Queue<int>();
                    queue.Enqueue(i);
                    while (queue.Count > 0) {
                        var u = queue.Dequeue();
                        currentComponent.Add(u);
                        foreach (var (v, _) in _representation.GetNeighbors(u))
                            if (visited.Add(v)) queue.Enqueue(v);
                    }
                    components.Add(currentComponent);
                }
            }
            return components.OrderByDescending(c => c.Count).ToList();
        }
        
        public Dictionary<string, double> GetDegreeMetrics() {
            if (_allDegreesCache == null) {
                _allDegreesCache = new List<int>(VertexCount);
                for (var i = 1; i <= VertexCount; i++) {
                    _allDegreesCache.Add(_representation.GetNeighbors(i).Count());
                }
            }
            var degrees = _allDegreesCache;
            if (degrees.Count == 0) 
                return new Dictionary<string, double> { { "min_degree", 0 }, { "max_degree", 0 }, { "avg_degree", 0 }, { "median_degree", 0 } };
            
            var sortedDegrees = degrees.OrderBy(d => d).ToList();
            var median = (sortedDegrees.Count % 2 == 1) ? sortedDegrees[sortedDegrees.Count / 2] : (sortedDegrees[sortedDegrees.Count / 2 - 1] + sortedDegrees[sortedDegrees.Count / 2]) / 2.0;
            return new Dictionary<string, double> { { "min_degree", degrees.Min() }, { "max_degree", degrees.Max() }, { "avg_degree", degrees.Average() }, { "median_degree", median } };
        }

        public (Dictionary<int, double> Distances, Dictionary<int, int?> Parents) Dijkstra(int startVertex) {
            if (HasNegativeWeights)
                throw new InvalidOperationException("A biblioteca não pode executar Dijkstra em grafos com pesos negativos.");
            
            var distances = new Dictionary<int, double>(VertexCount);
            var parents = new Dictionary<int, int?>(VertexCount);
            
            distances[startVertex] = 0;
            parents[startVertex] = null;
            
            _dijkstraStrategy.Initialize(VertexCount);
            _dijkstraStrategy.AddVertex(startVertex, 0);
            
            while (_dijkstraStrategy.TryGetNext(out var u, out var _)) {
                var distU = distances[u];
                
                foreach (var (neighbor, weight) in _representation.GetNeighbors(u)) {
                    var newDist = distU + weight;
                    
                    if (!distances.TryGetValue(neighbor, out var currentDist)) {
                        distances[neighbor] = newDist;
                        parents[neighbor] = u;
                        _dijkstraStrategy.AddVertex(neighbor, newDist);
                    } else if (newDist < currentDist) {
                        distances[neighbor] = newDist;
                        parents[neighbor] = u;
                        _dijkstraStrategy.AddVertex(neighbor, newDist);
                    }
                }
            }
            
            return (distances, parents);
        }

        public Graph CreateReversedCopy(Func<int, IGraphRepresentation> representationFactory = null,
                                        IDijkstraStrategy dijkstraStrategy = null) {
            var factory = representationFactory ?? _representationFactory;
            var reversed = new Graph(VertexCount, factory, dijkstraStrategy, _isDirected);

            for (var u = 1; u <= VertexCount; u++) {
                foreach (var (neighbor, weight) in _representation.GetNeighbors(u)) {
                    reversed.AddEdge(neighbor, u, weight);
                }
            }

            reversed.HasNegativeWeights = HasNegativeWeights;
            return reversed;
        }

        public BellmanFordResult BellmanFordFromSource(int startVertex) {
            ValidateVertexIndex(startVertex);
            return RunBellmanFord(startVertex);
        }

        public BellmanFordResult BellmanFordToTarget(int targetVertex) {
            ValidateVertexIndex(targetVertex);
            var reversed = CreateReversedCopy();
            var result = reversed.RunBellmanFord(targetVertex);
            return result with { Mode = BellmanFordResultMode.ToTarget };
        }

        private BellmanFordResult RunBellmanFord(int startVertex) {
            var distances = new Dictionary<int, double>(VertexCount);
            var parents = new Dictionary<int, int?>(VertexCount);

            for (var i = 1; i <= VertexCount; i++) {
                distances[i] = double.PositiveInfinity;
            }

            distances[startVertex] = 0;
            parents[startVertex] = null;

            var queue = new Queue<int>();
            var inQueue = new bool[VertexCount + 1];
            var enqueueCounts = new int[VertexCount + 1];

            queue.Enqueue(startVertex);
            inQueue[startVertex] = true;
            enqueueCounts[startVertex] = 1;

            var hasNegativeCycle = false;

            while (queue.Count > 0 && !hasNegativeCycle) {
                var u = queue.Dequeue();
                inQueue[u] = false;

                var distU = distances[u];
                if (double.IsPositiveInfinity(distU))
                    continue;

                foreach (var (neighbor, weight) in _representation.GetNeighbors(u)) {
                    var newDist = distU + weight;
                    if (newDist < distances[neighbor]) {
                        distances[neighbor] = newDist;
                        parents[neighbor] = u;

                        if (!inQueue[neighbor]) {
                            queue.Enqueue(neighbor);
                            inQueue[neighbor] = true;
                            enqueueCounts[neighbor]++;
                            if (enqueueCounts[neighbor] > VertexCount) {
                                hasNegativeCycle = true;
                                break;
                            }
                        }
                    }
                }
            }

            return new BellmanFordResult(distances, parents, hasNegativeCycle, startVertex, BellmanFordResultMode.FromSource);
        }

        public void LoadVertexNames(string filePath) {
            _vertexStringToInt.Clear();
            _vertexIntToString.Clear();
            foreach (var line in File.ReadLines(filePath)) {
                var parts = line.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out var id)) {
                    var name = parts[1].Trim();
                    _vertexStringToInt[name] = id;
                    _vertexIntToString[id] = name;
                }
            }
        }
        
        public int GetVertexId(string name) => _vertexStringToInt[name];
        public string GetVertexName(int id) => _vertexIntToString.GetValueOrDefault(id, $"ID {id}");

        public static Graph FromFileUnweighted(string filePath,
                                               Func<int, IGraphRepresentation> representationFactory,
                                               IDijkstraStrategy dijkstraStrategy = null,
                                               bool isDirected = false) {
            var lines = File.ReadAllLines(filePath);
            if (!int.TryParse(lines.FirstOrDefault(), out var vertexCount))
                throw new InvalidDataException("A primeira linha deve conter o número de vértices.");

            var graph = new Graph(vertexCount, representationFactory, dijkstraStrategy, isDirected);
            foreach (var line in lines.Skip(1)) {
                var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2 && int.TryParse(parts[0], out var u) && int.TryParse(parts[1], out var v)) {
                    graph.AddEdge(u, v);
                }
            }
            return graph;
        }

        public static Graph FromFileWeighted(string filePath,
                                             Func<int, IGraphRepresentation> representationFactory,
                                             IDijkstraStrategy dijkstraStrategy = null,
                                             bool isDirected = false) {
            var lines = File.ReadAllLines(filePath);
            if (!int.TryParse(lines.FirstOrDefault(), out var vertexCount))
                throw new InvalidDataException("A primeira linha deve conter o número de vértices.");

            var graph = new Graph(vertexCount, representationFactory, dijkstraStrategy, isDirected);
            var culture = CultureInfo.InvariantCulture;
            foreach (var line in lines.Skip(1)) {
                var parts = line.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 3 &&
                    int.TryParse(parts[0], out var u) &&
                    int.TryParse(parts[1], out var v) &&
                    double.TryParse(parts[2], NumberStyles.Float, culture, out var weight)) {
                    graph.AddEdge(u, v, weight);
                }
            }
            return graph;
        }
    }
}
