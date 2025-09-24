using System.Collections.Generic;

namespace GraphLibrary.Representations
{
    public interface IGraphRepresentation
    {
        int VertexCount { get; }
        int EdgeCount { get; }
        void AddEdge(int u, int v);
        IEnumerable<int> GetNeighbors(int v);
    }
}