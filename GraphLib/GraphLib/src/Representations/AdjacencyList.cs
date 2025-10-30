using System.Collections.Generic;
using System.Linq;

namespace GraphLibrary.Representations {
    public class AdjacencyList : IGraphRepresentation {
        private readonly Dictionary<int, Dictionary<int, double>> _adj;
        
        public int VertexCount { get; }
        public int EdgeCount { get; private set; }

        public AdjacencyList(int vertexCount) {
            VertexCount = vertexCount;
            EdgeCount = 0;
            _adj = new Dictionary<int, Dictionary<int, double>>(vertexCount);
            for (var i = 1; i <= vertexCount; i++) {
                _adj[i] = new Dictionary<int, double>();
            }
        }

        public void AddEdge(int u, int v, double weight) {
            if (!_adj[u].ContainsKey(v)) {
                EdgeCount++;
            }
            _adj[u][v] = weight;
            _adj[v][u] = weight;
        }

        public IEnumerable<(int vertex, double weight)> GetNeighbors(int v) {
            if (_adj.TryGetValue(v, out var neighbors)) {
                foreach (var kvp in neighbors) {
                    yield return (kvp.Key, kvp.Value);
                }
            }
        }
    }
}