using System.Collections.Generic;

namespace GraphLibrary.Representations
{
    /// <summary>
    /// Representação de grafo utilizando uma matriz de adjacência.
    /// Otimizada para grafos densos.
    /// </summary>
    public class AdjacencyMatrix : IGraphRepresentation
    {
        // Matriz bidimensional. Usamos byte (0-255) para economizar memória,
        // já que só precisamos dos valores 0 e 1.
        private readonly byte[,] _matrix;

        public int VertexCount { get; }
        public int EdgeCount { get; private set; }

        public AdjacencyMatrix(int vertexCount)
        {
            VertexCount = vertexCount;
            EdgeCount = 0;
            _matrix = new byte[vertexCount, vertexCount];
        }

        public void AddEdge(int u, int v)
        {
            // Vértices de 1 a N são mapeados para índices de 0 a N-1.
            int uIdx = u - 1;
            int vIdx = v - 1;

            if (_matrix[uIdx, vIdx] == 0)
            {
                _matrix[uIdx, vIdx] = 1;
                _matrix[vIdx, uIdx] = 1;
                EdgeCount++;
            }
        }

        public IEnumerable<int> GetNeighbors(int v)
        {
            var neighbors = new List<int>();
            int vIdx = v - 1; // Ajusta para o índice da matriz.

            for (int i = 0; i < VertexCount; i++)
            {
                if (_matrix[vIdx, i] == 1)
                {
                    neighbors.Add(i + 1); // Adiciona o rótulo do vértice (1 a N).
                }
            }
            return neighbors;
        }
    }
}