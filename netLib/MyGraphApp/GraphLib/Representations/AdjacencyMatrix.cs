using System.Collections.Generic;

namespace GraphLibrary.Representations
{
    public class AdjacencyMatrix : IGraphRepresentation
    {
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
            int vIdx = v - 1;
            for (int i = 0; i < VertexCount; i++)
            {
                if (_matrix[vIdx, i] == 1)
                {
                    // 'yield return' provides the next neighbor on demand without allocating a list.
                    yield return i + 1;
                }
            }
        }
    }
}