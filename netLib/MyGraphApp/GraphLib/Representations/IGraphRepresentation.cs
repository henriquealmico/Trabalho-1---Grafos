using System.Collections.Generic;

namespace GraphLibrary.Representations
{
    /// <summary>
    /// Interface (Contrato) que define os métodos e propriedades essenciais
    /// para qualquer representação de grafo.
    /// </summary>
    public interface IGraphRepresentation {
        int VertexCount { get; }
        int EdgeCount { get; }
        void AddEdge(int u, int v);
        IEnumerable<int> GetNeighbors(int v);
    }
}