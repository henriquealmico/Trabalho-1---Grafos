using System.Collections.Generic;

namespace GraphLibrary.Representations
{
    public class AdjacencyMatrix : IGraphRepresentation
    {
        // Alterado de byte[,] para double[,] para armazenar pesos reais [cite: 83]
        private readonly double[,] _matrix;
        private const double NoEdge = double.PositiveInfinity;

        public int VertexCount { get; }
        public int EdgeCount { get; private set; }

        public AdjacencyMatrix(int vertexCount)
        {
            VertexCount = vertexCount;
            EdgeCount = 0;
            _matrix = new double[vertexCount, vertexCount];
            // Inicializa a matriz com "Infinito" para marcar a ausência de arestas
            for (int i = 0; i < vertexCount; i++)
            {
                for (int j = 0; j < vertexCount; j++)
                {
                    _matrix[i, j] = NoEdge;
                }
            }
        }

        public void AddEdge(int u, int v, double weight)
        {
            int uIdx = u - 1;
            int vIdx = v - 1;

            if (_matrix[uIdx, vIdx] == NoEdge)
            {
                EdgeCount++;
            }
            _matrix[uIdx, vIdx] = weight;
            _matrix[vIdx, uIdx] = weight;
        }

        public IEnumerable<(int vertex, double weight)> GetNeighbors(int v)
        {
            int vIdx = v - 1;
            for (int i = 0; i < VertexCount; i++)
            {
                double weight = _matrix[vIdx, i];
                if (weight != NoEdge)
                {
                    // Retorna o vizinho (i+1) e o peso da aresta
                    yield return (i + 1, weight);
                }
            }
        }
    }
}