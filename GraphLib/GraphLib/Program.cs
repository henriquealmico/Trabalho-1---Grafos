using System.Diagnostics;
using GraphLibrary;
using GraphLibrary.Representations;

public class Program {
    private const string BasePath = @"C:\Users\halmi\Downloads\Trabalho 1 - Grafos\TestCases";

    public static void Main(string[] args) {
        var filePathsP1 = new List<string>();
        for (var i = 1; i <= 6; i++) {
            var path = Path.Combine(BasePath, $"grafo_{i}.txt");
            if (File.Exists(path)) filePathsP1.Add(path);
        }
        
        var graphFilesP2 = new List<string>(); 
        for (var i = 1; i <= 6; i++) {
            var path = Path.Combine(BasePath, $"grafo_W_{i}.txt");
            if (File.Exists(path)) graphFilesP2.Add(path);
        }
        
        Console.WriteLine(new string('#', 80));
        Console.WriteLine("### INICIANDO ESTUDOS DE CASO - PARTE 1 (Grafos Não Ponderados)");
        Console.WriteLine(new string('#', 80));
        
        Console.WriteLine("\n--- PARTE 1: ADJACENCY LIST ---");
        foreach (var filePath in filePathsP1) RunPart1Studies(filePath, v => new AdjacencyList(v));
        
        Console.WriteLine("\n--- PARTE 1: ADJACENCY MATRIX ---");
        foreach (var filePath in filePathsP1) RunPart1Studies(filePath, v => new AdjacencyMatrix(v));

        Console.WriteLine("\n\n" + new string('#', 80));
        Console.WriteLine("### INICIANDO ESTUDOS DE CASO - PARTE 2 (Grafos Ponderados)");
        Console.WriteLine(new string('#', 80));
        
        foreach (var filePath in graphFilesP2) RunPart2Studies(filePath);
        
        RunCollaborationNetworkStudy();
        
        Console.WriteLine("\n\n" + new string('#', 80));
        Console.WriteLine("### Todas as análises concluídas.");
        Console.WriteLine(new string('#', 80));
    }

    public static void RunPart1Studies(string filePath, Func<int, IGraphRepresentation> representationFactory) {
        var repType = representationFactory(1).GetType().Name;
        Console.WriteLine($"\n{new string('=', 80)}\nANALISANDO (P1): {Path.GetFileName(filePath)} | Usando: {repType}");
        try {
            GC.Collect();
            var memoryBefore = GC.GetTotalMemory(true);
            var graph = Graph.FromFileUnweighted(filePath, representationFactory);
            var memoryAfter = GC.GetTotalMemory(true);

            RunCaseStudiesPart1(graph, repType, memoryAfter - memoryBefore);
        }
        catch (OutOfMemoryException) {
            Console.WriteLine($"\n--- FALHA (P1) ---");
            Console.WriteLine("      [REASON] OutOfMemoryException: O grafo é muito grande para esta representação.");
        }
        catch (Exception ex) {
            Console.WriteLine($"\n[ERRO (P1)] Falha ao processar o arquivo {filePath}. Razão: {ex.Message}");
        }
    }

    public static void RunCaseStudiesPart1(Graph graph, string representationType, long memoryBytes) {
        Console.WriteLine($"\n--- Resultados (Parte 1) para: {representationType} ---");
        
        Console.WriteLine($"[1] Memory Usage: {memoryBytes / (1024.0 * 1024.0):F3} MB");
        Console.WriteLine("[2] BFS Average Time (100 runs): " + TimeSearchAlgorithm(graph, graph.BreadthFirstSearch, 100));
        Console.WriteLine("[3] DFS Average Time (100 runs): " + TimeSearchAlgorithm(graph, graph.DepthFirstSearch, 100));

        var verticesToTest = new[] { 10, 20, 30 };
        var startVertices = new[] { 1, 2, 3 };
        
        Console.WriteLine("[4] Parent Vertices:");
        foreach (var startNode in startVertices) {
            if (startNode > graph.VertexCount) continue;
            var (parentsBfs, _) = graph.BreadthFirstSearch(startNode);
            var (parentsDfs, _) = graph.DepthFirstSearch(startNode);
            foreach (var targetNode in verticesToTest) {
                if (targetNode > graph.VertexCount) continue;
                var bfsParent = parentsBfs.GetValueOrDefault(targetNode)?.ToString() ?? "N/A";
                var dfsParent = parentsDfs.GetValueOrDefault(targetNode)?.ToString() ?? "N/A";
                Console.WriteLine($"    - Start {startNode} -> Target {targetNode}: BFS Parent={bfsParent}, DFS Parent={dfsParent}");
            }
        }
        
        var pairsToTest = new[] { (10, 20), (10, 30), (20, 30) };
        Console.WriteLine("[5] Distances between pairs (unweighted):");
        foreach (var pair in pairsToTest) {
            if (pair.Item1 > graph.VertexCount || pair.Item2 > graph.VertexCount) continue;
            var distance = graph.GetDistance(pair.Item1, pair.Item2);
            Console.WriteLine($"    - Distance({pair.Item1}, {pair.Item2}): {(distance == -1 ? "Unreachable" : distance)}");
        }

        Console.WriteLine("[6] Connected Components:");
        var components = graph.GetConnectedComponents();
        if (components.Count != 0) {
            Console.WriteLine($"    - Number of components: {components.Count}");
            Console.WriteLine($"    - Largest component size: {components.First().Count}");
            Console.WriteLine($"    - Smallest component size: {components.Last().Count}");
        }
        
        Console.WriteLine("[7] Diameter (unweighted):");
        var stopwatch = Stopwatch.StartNew();
        var approxDiameter = graph.GetApproximateDiameter();
        stopwatch.Stop();
        Console.WriteLine($"    - Approximate Diameter: {approxDiameter} (took {stopwatch.ElapsedMilliseconds} ms)");
    }
    
    private static string TimeSearchAlgorithm(Graph graph, Action<int, Dictionary<int, int?>, Dictionary<int, int>> searchFunc, int runs) {
        if (graph.VertexCount == 0) return "N/A (empty graph)";
        var parents = new Dictionary<int, int?>(graph.VertexCount);
        var levels = new Dictionary<int, int>(graph.VertexCount);
        var random = new Random();
        var totalTime = TimeSpan.Zero;
        var stopwatch = new Stopwatch();
        for (var i = 0; i < runs; i++) {
            var startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            searchFunc(startVertex, parents, levels);
            stopwatch.Stop();
            totalTime += stopwatch.Elapsed;
        }
        return $"{totalTime.TotalMilliseconds / runs:F4} ms";
    }

    public static void RunPart2Studies(string filePath) {
        Console.WriteLine($"\n{new string('=', 80)}\nANALISANDO (P2): {Path.GetFileName(filePath)} | Usando: AdjacencyList");
        Graph graph;
        try {
            graph = Graph.FromFileWeighted(filePath, v => new AdjacencyList(v));
            if (graph.HasNegativeWeights) {
                Console.WriteLine("[AVISO] O grafo contém pesos negativos. O algoritmo de Dijkstra será pulado.");
                return;
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"\n[ERRO (P2)] Falha ao carregar o grafo {filePath}. Razão: {ex.Message}");
            return;
        }

        Console.WriteLine("\n[P2 - Estudo 3.1: Distâncias e Caminhos Mínimos]");
        const int startVertex = 10;
        var verticesToFind = new[] { 20, 30, 40, 50, 60 };
        var (distances, parents) = graph.Dijkstra(startVertex, useHeap: true);
        
        Console.WriteLine($"| Vértice Inicial | Vértice Final | Distância (Peso) | Caminho Mínimo (exemplos) |");
        Console.WriteLine($"|:--- |:--- |:--- |:--- |");
        foreach (var endVertex in verticesToFind) {
            if (endVertex > graph.VertexCount) continue;
            var dist = distances.GetValueOrDefault(endVertex, double.PositiveInfinity);
            var distStr = double.IsPositiveInfinity(dist) ? "Inalcançável" : $"{dist:F2}";
            var path = ReconstructPath(startVertex, endVertex, parents);
            var pathStr = path.Any() ? string.Join(" -> ", path.Take(5)) + (path.Count > 5 ? "..." : "") : "N/A";
            Console.WriteLine($"| {startVertex, -15} | {endVertex, -13} | {distStr, -16} | {pathStr,-25} |");
        }

        Console.WriteLine("\n[P2 - Estudo 3.2: Tempo de Execução Dijkstra (k=100)]");
        var k = 100;
        var timeArray = TimeDijkstra(graph, useHeap: false, k);
        var timeHeap = TimeDijkstra(graph, useHeap: true, k);
        Console.WriteLine($"| Implementação | Tempo Médio (k={k}) |");
        Console.WriteLine($"|:--- |:--- |");
        Console.WriteLine($"| Dijkstra com Vetor (O(V^2)) | {timeArray, -18} |");
        Console.WriteLine($"| Dijkstra com Heap (O((E+V)logV)) | {timeHeap, -18} |");
    }

    private static void RunCollaborationNetworkStudy() {
        Console.WriteLine($"\n{new string('=', 80)}\nANALISANDO (P2): Rede de Colaboração");
        var graphFile = Path.Combine(BasePath, "collab_graph.txt");
        var namesFile = Path.Combine(BasePath, "collab_names.txt");
        if (!File.Exists(graphFile) || !File.Exists(namesFile)) {
            Console.WriteLine("[AVISO] Arquivos 'collab_graph.txt' ou 'collab_names.txt' não encontrados. Pulando estudo.");
            return;
        }
        try {
            var graph = Graph.FromFileWeighted(graphFile, v => new AdjacencyList(v));
            graph.LoadVertexNames(namesFile);
            
            var startName = "Edsger W. Dijkstra";
            var targetNames = new[] { "Alan M. Turing", "J. B. Kruskal", "Jon M. Kleinberg", "Eva Tardos", "Daniel R. Figueiredo" };
            var startId = graph.GetVertexId(startName);
            var (distances, parents) = graph.Dijkstra(startId, useHeap: true);

            Console.WriteLine($"\nResultados do Caminho Mínimo a partir de '{startName}':");
            Console.WriteLine($"| Pesquisador Destino | Distância (Proximidade) | Caminho (exemplo) |");
            Console.WriteLine($"|:--- |:--- |:--- |");
            foreach (var targetName in targetNames) {
                var endId = graph.GetVertexId(targetName);
                var dist = distances.GetValueOrDefault(endId, double.PositiveInfinity);
                var distStr = double.IsPositiveInfinity(dist) ? "Inalcançável" : $"{dist:F4}";
                var pathIds = ReconstructPath(startId, endId, parents);
                var pathNames = pathIds.Select(id => graph.GetVertexName(id));
                var pathStr = pathIds.Any() ? string.Join(" -> ", pathNames.Take(4)) + "..." : "N/A";
                Console.WriteLine($"| {targetName,-20} | {distStr,-23} | {pathStr,-17} |");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"\n[ERRO (P2)] Falha ao processar a rede de colaboração. Razão: {ex.Message}");
        }
    }

    private static string TimeDijkstra(Graph graph, bool useHeap, int k) {
        if (graph.VertexCount == 0) return "N/A";
        var random = new Random();
        var totalTime = TimeSpan.Zero;
        var stopwatch = new Stopwatch();
        for (var i = 0; i < k; i++) {
            var startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            graph.Dijkstra(startVertex, useHeap);
            stopwatch.Stop();
            totalTime += stopwatch.Elapsed;
        }
        return $"{(totalTime.TotalMilliseconds / k):F4} ms";
    }

    private static List<int> ReconstructPath(int start, int end, Dictionary<int, int?> parents) {
        var path = new List<int>();
        int? current = end;
        while (current.HasValue && parents.ContainsKey(current.Value)) {
            path.Add(current.Value);
            if (current.Value == start) break;
            current = parents[current.Value];
        }
        if (current.HasValue && current.Value == start) {
            if (path.Last() != start) path.Add(start);
            path.Reverse();
            return path;
        }
        return [];
    }
}
