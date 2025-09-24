using System.Collections.Generic;
using System.Linq;

namespace GraphLibrary.Representations
{
    public class AdjacencyList : IGraphRepresentation
    {
        private readonly Dictionary<int, HashSet<int>> _adj;
        
        public int VertexCount { get; }
        public int EdgeCount { get; private set; }

        public AdjacencyList(int vertexCount)
        {
            VertexCount = vertexCount;
            EdgeCount = 0;
            _adj = new Dictionary<int, HashSet<int>>(vertexCount);
            for (int i = 1; i <= vertexCount; i++)
            {
                _adj[i] = new HashSet<int>();
            }
        }

        public void AddEdge(int u, int v)
        {
            if (_adj[u].Add(v))
            {
                _adj[v].Add(u);
                EdgeCount++;
            }
        }

        public IEnumerable<int> GetNeighbors(int v)
        {
            return _adj.GetValueOrDefault(v, new HashSet<int>());
        }
    }
}