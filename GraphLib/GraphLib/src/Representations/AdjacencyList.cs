using System.Collections.Generic;
using System.Linq;

namespace GraphLibrary.Representations
{
    public class AdjacencyList : IGraphRepresentation
    {
        // Alterado de HashSet<int> para Dictionary<int, double> para armazenar o peso.
        // Chave externa: vértice u
        // Chave interna: vértice v (vizinho)
        // Valor interno: peso da aresta (u, v)
        private readonly Dictionary<int, Dictionary<int, double>> _adj;
        
        public int VertexCount { get; }
        public int EdgeCount { get; private set; }

        public AdjacencyList(int vertexCount)
        {
            VertexCount = vertexCount;
            EdgeCount = 0;
            _adj = new Dictionary<int, Dictionary<int, double>>(vertexCount);
            for (int i = 1; i <= vertexCount; i++)
            {
                _adj[i] = new Dictionary<int, double>();
            }
        }

        public void AddEdge(int u, int v, double weight)
        {
            // Adiciona a aresta nos dois sentidos, já que o grafo não é direcionado 
            if (!_adj[u].ContainsKey(v))
            {
                EdgeCount++;
            }
            _adj[u][v] = weight;
            _adj[v][u] = weight;
        }

        public IEnumerable<(int vertex, double weight)> GetNeighbors(int v)
        {
            if (!_adj.ContainsKey(v))
            {
                return Enumerable.Empty<(int, double)>();
            }
            // Transforma o dicionário de vizinhos em uma coleção de tuplas
            return _adj[v].Select(kvp => (kvp.Key, kvp.Value));
        }
    }
}