using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GraphLibrary.Representations;

namespace GraphLibrary
{
    public class Graph
    {
        private readonly IGraphRepresentation _representation;
        private List<int> _allDegreesCache;
        
        // Mapeamentos para o estudo de caso da rede de colaboração 
        private readonly Dictionary<string, int> _vertexStringToInt = new();
        private readonly Dictionary<int, string> _vertexIntToString = new();

        public int VertexCount => _representation.VertexCount;
        public int EdgeCount => _representation.EdgeCount;
        public bool HasNegativeWeights { get; private set; } = false;

        public Graph(int vertexCount, Func<int, IGraphRepresentation> representationFactory)
        {
            if (vertexCount <= 0)
                throw new ArgumentException("O número de vértices deve ser positivo.", nameof(vertexCount));
            
            _representation = representationFactory(vertexCount);
        }

        private void InvalidateDegreeCache() => _allDegreesCache = null;

        // Atualizado para aceitar pesos
        public void AddEdge(int u, int v, double weight)
        {
            if (u < 1 || u > VertexCount || v < 1 || v > VertexCount)
                throw new ArgumentOutOfRangeException($"Vértices devem estar no intervalo [1, {VertexCount}].");
            
            if (weight < 0)
            {
                HasNegativeWeights = true;
            }
            
            _representation.AddEdge(u, v, weight);
            InvalidateDegreeCache();
        }

        public Dictionary<string, double> GetDegreeMetrics()
        {
            // ... (código de GetDegreeMetrics não modificado, continua funcionando) ...
            if (_allDegreesCache == null)
            {
                _allDegreesCache = new List<int>(VertexCount);
                for (int i = 1; i <= VertexCount; i++) _allDegreesCache.Add(_representation.GetNeighbors(i).Count());
            }
            var degrees = _allDegreesCache;
            if (!degrees.Any()) return new Dictionary<string, double> { { "min_degree", 0 }, { "max_degree", 0 }, { "avg_degree", 0 }, { "median_degree", 0 } };
            var sortedDegrees = degrees.OrderBy(d => d).ToList();
            double median = (sortedDegrees.Count % 2 == 1) ? sortedDegrees[sortedDegrees.Count / 2] : (sortedDegrees[sortedDegrees.Count / 2 - 1] + sortedDegrees[sortedDegrees.Count / 2]) / 2.0;
            return new Dictionary<string, double> { { "min_degree", degrees.Min() }, { "max_degree", degrees.Max() }, { "avg_degree", degrees.Average() }, { "median_degree", median } };
        }

        // --- Algoritmos de Caminho Mínimo (Dijkstra) ---
        
        public (Dictionary<int, double> Distances, Dictionary<int, int?> Parents) Dijkstra(int startVertex, bool useHeap)
        {
            if (HasNegativeWeights)
            {
                // Conforme solicitado, informa que não implementa com pesos negativos 
                throw new InvalidOperationException("A biblioteca ainda não implementa caminhos mínimos com pesos negativos. O algoritmo de Dijkstra não pode ser executado.");
            }
            
            if (useHeap)
            {
                return DijkstraWithHeap(startVertex); // Implementação O((E+V) log V) [cite: 90]
            }
            else
            {
                return DijkstraWithArray(startVertex); // Implementação O(V^2) [cite: 89]
            }
        }

        /// <summary>
        /// Implementação de Dijkstra usando um vetor para encontrar o próximo vértice (O(V^2)). [cite: 89]
        /// </summary>
        private (Dictionary<int, double> Distances, Dictionary<int, int?> Parents) DijkstraWithArray(int startVertex)
        {
            var distances = new Dictionary<int, double>(VertexCount);
            var parents = new Dictionary<int, int?>(VertexCount);
            var visited = new HashSet<int>();

            for (int i = 1; i <= VertexCount; i++)
            {
                distances[i] = double.PositiveInfinity;
                parents[i] = null;
            }
            distances[startVertex] = 0;

            for (int i = 0; i < VertexCount; i++)
            {
                // Encontra o vértice não visitado com a menor distância (operação O(V))
                int u = -1;
                double minDistance = double.PositiveInfinity;
                foreach (var v in distances)
                {
                    if (!visited.Contains(v.Key) && v.Value < minDistance)
                    {
                        minDistance = v.Value;
                        u = v.Key;
                    }
                }

                if (u == -1 || minDistance == double.PositiveInfinity)
                    break; // Todos os vértices restantes são inalcançáveis

                visited.Add(u);

                // Relaxamento
                foreach (var (neighbor, weight) in _representation.GetNeighbors(u))
                {
                    double newDist = distances[u] + weight;
                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        parents[neighbor] = u;
                    }
                }
            }
            return (distances, parents);
        }

        /// <summary>
        /// Implementação de Dijkstra usando uma Fila de Prioridade (Heap) (O((E+V) log V)). [cite: 90]
        /// </summary>
        private (Dictionary<int, double> Distances, Dictionary<int, int?> Parents) DijkstraWithHeap(int startVertex)
        {
            var distances = new Dictionary<int, double>(VertexCount);
            var parents = new Dictionary<int, int?>(VertexCount);
            var pq = new PriorityQueue<int, double>(); // Usa a PriorityQueue nativa do .NET
            var visited = new HashSet<int>();

            for (int i = 1; i <= VertexCount; i++)
            {
                distances[i] = double.PositiveInfinity;
                parents[i] = null;
            }
            distances[startVertex] = 0;
            pq.Enqueue(startVertex, 0);

            while (pq.TryDequeue(out int u, out double priority))
            {
                // Se já visitamos, pulamos (essa é uma entrada "antiga" na fila)
                if (!visited.Add(u))
                    continue;

                // Otimização: se a prioridade (distância) na fila for maior que a
                // distância já conhecida, é uma entrada antiga e pode ser ignorada.
                if (priority > distances[u])
                    continue;

                // Relaxamento
                foreach (var (neighbor, weight) in _representation.GetNeighbors(u))
                {
                    if (visited.Contains(neighbor)) continue;

                    double newDist = distances[u] + weight;
                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        parents[neighbor] = u;
                        // Adicionamos o vizinho à fila. Não há "decrease-key",
                        // então podemos adicionar múltiplas entradas para o mesmo vértice [cite: 91]
                        pq.Enqueue(neighbor, newDist);
                    }
                }
            }
            return (distances, parents);
        }

        // --- Métodos de Mapeamento (Estudo de Caso 3) ---

        /// <summary>
        /// Carrega um arquivo de mapeamento (ex: "id,nome") para o estudo de caso da rede.
        /// </summary>
        public void LoadVertexNames(string filePath)
        {
            _vertexStringToInt.Clear();
            _vertexIntToString.Clear();
            foreach (var line in File.ReadLines(filePath))
            {
                var parts = line.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out int id))
                {
                    string name = parts[1].Trim();
                    _vertexStringToInt[name] = id;
                    _vertexIntToString[id] = name;
                }
            }
        }

        public int GetVertexId(string name)
        {
            if (!_vertexStringToInt.ContainsKey(name))
                throw new KeyNotFoundException($"O vértice com nome '{name}' não foi encontrado no mapeamento.");
            return _vertexStringToInt[name];
        }

        public string GetVertexName(int id)
        {
            return _vertexIntToString.GetValueOrDefault(id, $"ID {id} desconhecido");
        }

        // --- Método de Fábrica (Atualizado) ---

        public static Graph FromFile(string filePath, Func<int, IGraphRepresentation> representationFactory)
        {
            var lines = File.ReadAllLines(filePath);
            if (!int.TryParse(lines.FirstOrDefault(), out int vertexCount))
                throw new InvalidDataException("A primeira linha deve conter o número de vértices.");

            var graph = new Graph(vertexCount, representationFactory);
            
            // Usar InvariantCulture para garantir que '.' seja lido como decimal [cite: 83, 85]
            var culture = CultureInfo.InvariantCulture; 

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                
                // O formato agora tem 3 colunas 
                if (parts.Length == 3 && 
                    int.TryParse(parts[0], out int u) && 
                    int.TryParse(parts[1], out int v) && 
                    double.TryParse(parts[2], NumberStyles.Float, culture, out double weight))
                {
                    graph.AddEdge(u, v, weight);
                }
            }
            return graph;
        }

        // Métodos da Parte 1 (BFS, DFS, Componentes, etc.) não são mais diretamente
        // usados pelos novos estudos de caso, mas podem ser mantidos para retrocompatibilidade
        // (embora precisassem de ajustes para ignorar os pesos).
        // Para focar na Parte 2, eles foram omitidos deste arquivo.
    }
}