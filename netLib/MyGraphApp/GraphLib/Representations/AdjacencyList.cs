using System.Collections.Generic;
using System.Linq;

namespace GraphLibrary.Representations
{
    /// <summary>
    /// Representação de grafo utilizando uma lista de adjacência.
    /// Otimizada para grafos esparsos.
    /// </summary>
    public class AdjacencyList : IGraphRepresentation
    {
        // Um dicionário onde a chave é o vértice e o valor é um HashSet de seus vizinhos.
        // HashSet garante performance O(1) para adições e verificações de existência.
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
            // O método Add do HashSet retorna true apenas se o item não existia,
            // evitando contagem dupla de arestas e garantindo a idempotência.
            if (_adj[u].Add(v))
            {
                _adj[v].Add(u);
                EdgeCount++;
            }
        }

        public IEnumerable<int> GetNeighbors(int v)
        {
            // Retorna a coleção de vizinhos. Se v for inválido, lança uma exceção.
            return _adj.ContainsKey(v) ? _adj[v] : Enumerable.Empty<int>();
        }
    }
}