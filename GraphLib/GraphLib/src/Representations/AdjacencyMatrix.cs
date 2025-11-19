using System.Collections.Generic;

namespace GraphLibrary.Representations {
    public class AdjacencyMatrix : IGraphRepresentation {
        private readonly double[,] _matrix;
        private const double NoEdge = double.PositiveInfinity;

        public int VertexCount { get; }
        public int EdgeCount { get; private set; }

        public AdjacencyMatrix(int vertexCount) {
            VertexCount = vertexCount;
            EdgeCount = 0;
            _matrix = new double[vertexCount, vertexCount];
            for (var i = 0; i < vertexCount; i++) {
                for (var j = 0; j < vertexCount; j++) {
                    _matrix[i, j] = NoEdge;
                }
            }
        }

        public void AddEdge(int u, int v, double weight, bool isDirected) {
            var uIdx = u - 1;
            var vIdx = v - 1;

            if (_matrix[uIdx, vIdx] == NoEdge) {
                EdgeCount++;
            }
            _matrix[uIdx, vIdx] = weight;

            if (!isDirected) {
                _matrix[vIdx, uIdx] = weight;
            }
        }

        public IEnumerable<(int vertex, double weight)> GetNeighbors(int v) {
            var vIdx = v - 1;
            for (var i = 0; i < VertexCount; i++) {
                var weight = _matrix[vIdx, i];
                if (weight != NoEdge)
                    yield return (i + 1, weight);
            }
        }
    }
}