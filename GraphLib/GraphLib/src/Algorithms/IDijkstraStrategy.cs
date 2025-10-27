namespace GraphLibrary.Algorithms {
    public interface IDijkstraStrategy {
        void Initialize(int vertexCount);
        void AddVertex(int vertex, double distance);
        bool TryGetNext(out int vertex, out double distance);
        bool IsEmpty();
    }
}
