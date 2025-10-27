namespace GraphLibrary.Algorithms {
    public class DijkstraArrayStrategy : IDijkstraStrategy {
        private Dictionary<int, double> _distances;
        private HashSet<int> _visited;

        public void Initialize(int vertexCount) {
            _distances = new Dictionary<int, double>(vertexCount);
            _visited = new HashSet<int>(vertexCount);
        }

        public void AddVertex(int vertex, double distance) {
            _distances[vertex] = distance;
        }

        public bool TryGetNext(out int vertex, out double distance) {
            vertex = -1;
            distance = double.PositiveInfinity;

            foreach (var kvp in _distances) {
                var v = kvp.Key;
                var d = kvp.Value;
                if (!_visited.Contains(v) && d < distance) {
                    distance = d;
                    vertex = v;
                }
            }

            if (vertex == -1) {
                return false;
            }

            _visited.Add(vertex);
            return true;
        }

        public bool IsEmpty() {
            return _visited.Count >= _distances.Count;
        }
    }
}
