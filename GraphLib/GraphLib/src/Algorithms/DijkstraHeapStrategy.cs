namespace GraphLibrary.Algorithms {
    public class DijkstraHeapStrategy : IDijkstraStrategy {
        private PriorityQueue<int, double> _priorityQueue;
        private HashSet<int> _visited;

        public void Initialize(int vertexCount) {
            _priorityQueue = new PriorityQueue<int, double>(vertexCount);
            _visited = new HashSet<int>(vertexCount);
        }

        public void AddVertex(int vertex, double distance) {
            _priorityQueue.Enqueue(vertex, distance);
        }

        public bool TryGetNext(out int vertex, out double distance) {
            while (_priorityQueue.TryDequeue(out vertex, out distance)) {
                if (_visited.Add(vertex)) {
                    return true;
                }
            }

            vertex = -1;
            distance = double.PositiveInfinity;
            return false;
        }

        public bool IsEmpty() {
            return _priorityQueue.Count == 0;
        }
    }
}
