using System.Collections.Generic;

namespace GraphLibrary.Representations
{
    /// <summary>
    /// Interface atualizada para suportar grafos com pesos.
    /// </summary>
    public interface IGraphRepresentation
    {
        int VertexCount { get; }
        int EdgeCount { get; }
        
        /// <summary>
        /// Adiciona uma aresta ponderada entre os vértices u e v.
        /// </summary>
        void AddEdge(int u, int v, double weight);

        /// <summary>
        /// Retorna todos os vizinhos de um vértice e o peso da aresta para cada vizinho.
        /// </summary>
        /// <returns>Uma coleção de tuplas (vizinho, peso).</returns>
        IEnumerable<(int vertex, double weight)> GetNeighbors(int v);
    }
}