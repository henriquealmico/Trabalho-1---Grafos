using System.Collections.Generic;

namespace GraphLibrary.Representations {
    public interface IGraphRepresentation {
        int VertexCount { get; }
        int EdgeCount { get; }
        
        void AddEdge(int u, int v, double weight);

        IEnumerable<(int vertex, double weight)> GetNeighbors(int v);
    }
}