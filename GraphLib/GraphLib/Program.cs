using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GraphLibrary;
using GraphLibrary.Algorithms;
using GraphLibrary.Representations;

public class Program {
    private const string BasePath = @"C:\Users\halmi\Downloads\Trabalho 1 - Grafos\TestCases";
    private const int MaxGraphFiles = 15;
    private const int BfsRunCount = 100;
    private const int DijkstraRunCount = 100;
    private const int BellmanFordRunCount = 10;
    
    private static class FeatureToggles {
        // PART 1 - Unweighted Graphs
        public const bool RunPart1 = false;
        public const bool RunPart1AdjacencyList = false;
        public const bool RunPart1AdjacencyMatrix = false;
        
        // PART 2 - Weighted Graphs
        public const bool RunPart2 = false;
        public const bool RunPart2WeightedGraphs = false;
        public const bool RunPart2CollaborationNetwork = false;

        // PART 3 - Directed Graph Studies
        public const bool RunPart3 = true;
        public const bool RunPart3CaseStudies = true;
    }

    private static class GraphInputOptions {
        public static bool IsDirected { get; private set; }

        public static void Configure(string[]? args) {
            IsDirected = false;
            if (args is null) return;

            foreach (var arg in args) {
                if (arg.Equals("--directed", StringComparison.OrdinalIgnoreCase)) {
                    IsDirected = true;
                } else if (arg.Equals("--undirected", StringComparison.OrdinalIgnoreCase)) {
                    IsDirected = false;
                } else if (arg.StartsWith("--directed=", StringComparison.OrdinalIgnoreCase)) {
                    if (bool.TryParse(arg.Split('=')[1], out var parsed)) {
                        IsDirected = parsed;
                    }
                }
            }
        }
    }

    public static void Main(string[]? args) {
        GraphInputOptions.Configure(args);
        PrintHeader();
        
        if (FeatureToggles.RunPart1)
            RunUnweightedGraphs();

        if (FeatureToggles.RunPart2)
            RunWeightedGraphs();

        if (FeatureToggles.RunPart3)
            RunCaseStudyGraphs();
        
        PrintFooter();
    }

    // ========================= MAIN TEST RUNNERS =========================
    
    private static void RunUnweightedGraphs() {
        var files = GetGraphFiles("grafo_{0}.txt");
        
        PrintSectionHeader("PARTE 1 - GRAFOS NÃO PONDERADOS");
        
        if (FeatureToggles.RunPart1AdjacencyList) {
            Console.WriteLine("\n--- ADJACENCY LIST ---");
            foreach (var file in files) {
                RunPart1Studies(file, v => new AdjacencyList(v));
            }
        }
        
        if (FeatureToggles.RunPart1AdjacencyMatrix) {
            Console.WriteLine("\n--- ADJACENCY MATRIX ---");
            foreach (var file in files) {
                RunPart1Studies(file, v => new AdjacencyMatrix(v));
            }
        }
    }

    private static void RunWeightedGraphs() {
        PrintSectionHeader("PARTE 2 - GRAFOS PONDERADOS");
        
        if (FeatureToggles.RunPart2WeightedGraphs) {
            var files = GetGraphFiles("grafo_W_{0}.txt");
            foreach (var file in files) 
                RunPart2Studies(file);
        }
        
        if (FeatureToggles.RunPart2CollaborationNetwork)
            RunCollaborationNetworkStudy();
    }

    // ========================= PART 1 TESTS =========================

    private static void RunPart1Studies(string filePath, Func<int, IGraphRepresentation> representationFactory) {
        var repType = representationFactory(1).GetType().Name;
        PrintFileHeader(filePath, repType, "P1");
        
        try {
            var (graph, memoryUsed) = LoadGraphWithMemoryTracking(
                filePath,
                representationFactory,
                Graph.FromFileUnweighted,
                GraphInputOptions.IsDirected
            );
            
            ExecutePart1Tests(graph, repType, memoryUsed);
        }
        catch (OutOfMemoryException) {
            PrintError("OutOfMemoryException: O grafo é muito grande para esta representação.");
        }
        catch (Exception ex) {
            PrintError($"Falha ao processar: {ex.Message}");
        }
    }

    private static void ExecutePart1Tests(Graph graph, string representationType, long memoryBytes) {
        Console.WriteLine($"\n--- Resultados para: {representationType} ---");
        
        // Test 1: Memory Usage
        PrintTestResult("Memory Usage", $"{memoryBytes / (1024.0 * 1024.0):F3} MB");
        
        // Test 2 & 3: Algorithm Performance
        var bfsTime = MeasureSearchAlgorithm(graph, graph.BreadthFirstSearch, BfsRunCount);
        var dfsTime = MeasureSearchAlgorithm(graph, graph.DepthFirstSearch, BfsRunCount);
        PrintTestResult($"BFS Average Time ({BfsRunCount} runs)", bfsTime);
        PrintTestResult($"DFS Average Time ({BfsRunCount} runs)", dfsTime);

        // Test 4: Parent Vertices
        TestParentVertices(graph);
        
        // Test 5: Distances
        TestDistances(graph);

        // Test 6: Connected Components
        TestConnectedComponents(graph);
        
        // Test 7: Diameter
        TestDiameter(graph);
    }

    // ========================= PART 2 TESTS =========================

    private static void RunPart2Studies(string filePath) {
        PrintFileHeader(filePath, "AdjacencyList", "P2");
        
        try {
            var graph = Graph.FromFileWeighted(
                filePath,
                v => new AdjacencyList(v),
                new GraphLibrary.Algorithms.DijkstraHeapStrategy(),
                GraphInputOptions.IsDirected);
            
            if (graph.HasNegativeWeights) {
                Console.WriteLine("[AVISO] Grafo com pesos negativos. Dijkstra não será executado.");
                return;
            }

            ExecuteDijkstraTests(graph, filePath);
        }
        catch (Exception ex) {
            PrintError($"Falha ao carregar: {ex.Message}");
        }
    }

    private static void ExecuteDijkstraTests(Graph graph, string filePath) {
        const int startVertex = 10;
        var targetVertices = new[] { 20, 30, 40, 50, 60 };
        
        Console.WriteLine("\n[Estudo 3.1: Distâncias e Caminhos Mínimos]");
        var (distances, parents) = graph.Dijkstra(startVertex);
        
        Console.WriteLine("| Vértice Inicial | Vértice Final | Distância | Caminho Mínimo |");
        Console.WriteLine(new string('-', 70));
        
        foreach (var endVertex in targetVertices.Where(v => v <= graph.VertexCount)) {
            var dist = distances.GetValueOrDefault(endVertex, double.PositiveInfinity);
            var distStr = double.IsPositiveInfinity(dist) ? "Inalcançável" : $"{dist:F2}";
            var path = ReconstructPath(startVertex, endVertex, parents);
            var pathStr = FormatPath(path);
            
            Console.WriteLine($"| {startVertex,-15} | {endVertex,-13} | {distStr,-9} | {pathStr,-22} |");
        }

        Console.WriteLine($"\n[Estudo 3.2: Tempo de Execução Dijkstra (k={DijkstraRunCount})]");
        var timeArray = MeasureDijkstraWithStrategy(filePath, new GraphLibrary.Algorithms.DijkstraArrayStrategy(), DijkstraRunCount);
        var timeHeap = MeasureDijkstraWithStrategy(filePath, new GraphLibrary.Algorithms.DijkstraHeapStrategy(), DijkstraRunCount);
        
        Console.WriteLine("| Implementação | Tempo Médio |");
        Console.WriteLine(new string('-', 50));
        Console.WriteLine($"| Dijkstra com Vetor (O(V²)) | {timeArray,-18} |");
        Console.WriteLine($"| Dijkstra com Heap (O((E+V)logV)) | {timeHeap,-18} |");
    }

    private static void RunCollaborationNetworkStudy() {
        Console.WriteLine($"\n{new string('=', 80)}\nANALISANDO (P2): Rede de Colaboração");
        var graphFile = Path.Combine(BasePath, "rede_colaboracao.txt");
        var namesFile = Path.Combine(BasePath, "rede_colaboracao_vertices.txt");
        if (!File.Exists(graphFile) || !File.Exists(namesFile)) {
            Console.WriteLine("[AVISO] Arquivos 'rede_colaboracao.txt' ou 'rede_colaboracao_vertices.txt' não encontrados. Pulando estudo.");
            return;
        }
        try {
            var graph = Graph.FromFileWeighted(
                graphFile,
                v => new AdjacencyList(v),
                new GraphLibrary.Algorithms.DijkstraHeapStrategy(),
                GraphInputOptions.IsDirected);
            graph.LoadVertexNames(namesFile);
            
            const string startName = "Edsger W. Dijkstra";
            var targetNames = new[] { "Alan M. Turing", "J. B. Kruskal", "Jon M. Kleinberg", "Éva Tardos", "Daniel R. Figueiredo" };
            var startId = graph.GetVertexId(startName);
            var (distances, parents) = graph.Dijkstra(startId);

            Console.WriteLine($"\nResultados do Caminho Mínimo a partir de '{startName}':");
            Console.WriteLine($"| Pesquisador Destino | Distância (Proximidade) | Caminho (exemplo) |");
            foreach (var targetName in targetNames) {
                var endId = graph.GetVertexId(targetName);
                var dist = distances.GetValueOrDefault(endId, double.PositiveInfinity);
                var distStr = double.IsPositiveInfinity(dist) ? "Inalcançável" : $"{dist:F4}";
                var pathIds = ReconstructPath(startId, endId, parents);
                var pathNames = pathIds.Select(id => graph.GetVertexName(id));
                var pathStr = pathIds.Count != 0 ? string.Join(" -> ", pathNames.Take(4)) + "..." : "N/A";
                Console.WriteLine($"| {targetName,-20} | {distStr,-23} | {pathStr,-17} |");
            }
        }
        catch (Exception ex) {
            Console.WriteLine($"\n[ERRO (P2)] Falha ao processar a rede de colaboração. Razão: {ex.Message}");
        }
    }

    // ========================= PART 3 CASE STUDIES =========================

    private static void RunCaseStudyGraphs() {
        if (!FeatureToggles.RunPart3CaseStudies)
            return;

        var files = GetGraphFiles("grafo_W_{0}.txt");
        if (files.Count == 0) {
            Console.WriteLine("[AVISO] Nenhum arquivo de estudo de caso (grafo_W_*) foi encontrado.");
            return;
        }

        PrintSectionHeader("PARTE 3 - ESTUDOS DE CASO (Bellman-Ford)");
        foreach (var file in files) {
            RunCaseStudyForGraph(file);
        }
    }

    private static void RunCaseStudyForGraph(string filePath) {
        PrintFileHeader(filePath, "AdjacencyList", "P3");
        const int targetVertex = 100;
        var requestedSources = new[] { 10, 20, 30 };

        try {
            var graph = Graph.FromFileWeighted(
                filePath,
                v => new AdjacencyList(v),
                new GraphLibrary.Algorithms.DijkstraHeapStrategy(),
                GraphInputOptions.IsDirected);

            if (targetVertex > graph.VertexCount) {
                Console.WriteLine($"[AVISO] Vértice alvo {targetVertex} não existe neste grafo.");
                return;
            }

            var sourceVertices = requestedSources.Where(v => v <= graph.VertexCount).ToArray();
            if (sourceVertices.Length == 0) {
                Console.WriteLine("[AVISO] Vértices 10, 20 e 30 não estão presentes neste grafo.");
                return;
            }

            var bellmanResult = graph.BellmanFordToTarget(targetVertex);

            PrintBellmanFordDistances(sourceVertices, targetVertex, bellmanResult);

            var bellmanSeconds = MeasureBellmanFord(graph, targetVertex, BellmanFordRunCount);
            PrintAverageTimeTable("Bellman-Ford", bellmanSeconds);

            if (bellmanResult.HasNegativeCycle) {
                Console.WriteLine("[ALERTA] Ciclo negativo detectado. Resultados podem não representar distâncias reais.");
                return;
            }

            if (graph.HasNegativeWeights) {
                Console.WriteLine("[AVISO] Pesos negativos detectados. Comparação com Dijkstra não será executada.");
                return;
            }

            var reversedGraph = graph.CreateReversedCopy();
            var dijkstraResult = reversedGraph.Dijkstra(targetVertex);
            var dijkstraSeconds = MeasureDijkstraAtTarget(reversedGraph, targetVertex, BellmanFordRunCount);

            PrintComparisonTables(
                sourceVertices,
                bellmanResult,
                dijkstraResult,
                bellmanSeconds,
                dijkstraSeconds);
        }
        catch (Exception ex) {
            PrintError($"Falha no estudo de caso: {ex.Message}");
        }
    }

    private static void PrintBellmanFordDistances(IEnumerable<int> sources, int targetVertex, BellmanFordResult result) {
        Console.WriteLine("\n[Estudo 3.1: Distâncias até o vértice 100 (Bellman-Ford)]");
        Console.WriteLine("| Vértice Fonte | Vértice Alvo | Distância | Caminho |");
        Console.WriteLine(new string('-', 80));

        foreach (var source in sources) {
            var dist = result.Distances.GetValueOrDefault(source, double.PositiveInfinity);
            var distStr = FormatDistance(dist);
            var path = ReconstructPathToTarget(source, targetVertex, result.Parents);
            var pathStr = FormatPath(path);
            Console.WriteLine($"| {source,-13} | {targetVertex,-12} | {distStr,-9} | {pathStr,-40} |");
        }

        Console.WriteLine(new string('-', 80));
        Console.WriteLine($"Ciclo negativo detectado? {(result.HasNegativeCycle ? "SIM" : "NÃO")}");
    }

    private static void PrintAverageTimeTable(string algorithmName, double seconds) {
        Console.WriteLine($"\n[Estudo 3.2: Tempo Médio ({BellmanFordRunCount} execuções)]");
        Console.WriteLine("| Algoritmo | Tempo Médio (s) |");
        Console.WriteLine(new string('-', 45));
        Console.WriteLine($"| {algorithmName,-10} | {seconds:F6,-17} |");
    }

    private static void PrintComparisonTables(
        IEnumerable<int> sources,
        BellmanFordResult bellmanResult,
        (Dictionary<int, double> Distances, Dictionary<int, int?> Parents) dijkstraResult,
        double bellmanSeconds,
        double dijkstraSeconds) {

        Console.WriteLine("\n[Estudo 3.3: Comparação Bellman-Ford x Dijkstra]");
        Console.WriteLine("| Vértice | Bellman-Ford | Dijkstra | Δ |");
        Console.WriteLine(new string('-', 65));

        foreach (var source in sources) {
            var bellman = bellmanResult.Distances.GetValueOrDefault(source, double.PositiveInfinity);
            var dijkstra = dijkstraResult.Distances.GetValueOrDefault(source, double.PositiveInfinity);
            var bellmanStr = FormatDistance(bellman);
            var dijkstraStr = FormatDistance(dijkstra);
            var delta = (double.IsPositiveInfinity(bellman) || double.IsPositiveInfinity(dijkstra))
                ? "N/A"
                : $"{Math.Abs(bellman - dijkstra):F4}";
            Console.WriteLine($"| {source,-7} | {bellmanStr,-13} | {dijkstraStr,-9} | {delta,-6} |");
        }

        Console.WriteLine("\n| Algoritmo | Tempo Médio (s) |");
        Console.WriteLine(new string('-', 45));
        Console.WriteLine($"| Bellman-Ford | {bellmanSeconds:F6,-17} |");
        Console.WriteLine($"| Dijkstra (grafo invertido) | {dijkstraSeconds:F6,-17} |");
    }

    // ========================= TEST HELPERS =========================

    private static void TestParentVertices(Graph graph) {
        Console.WriteLine("[4] Parent Vertices:");
        var startVertices = new[] { 1, 2, 3 };
        var targetVertices = new[] { 10, 20, 30 };
        
        foreach (var start in startVertices.Where(v => v <= graph.VertexCount)) {
            var (parentsBfs, _) = graph.BreadthFirstSearch(start);
            var (parentsDfs, _) = graph.DepthFirstSearch(start);
            
            foreach (var target in targetVertices.Where(v => v <= graph.VertexCount)) {
                var bfsParent = parentsBfs.GetValueOrDefault(target)?.ToString() ?? "N/A";
                var dfsParent = parentsDfs.GetValueOrDefault(target)?.ToString() ?? "N/A";
                Console.WriteLine($"    Start {start} → Target {target}: BFS={bfsParent}, DFS={dfsParent}");
            }
        }
    }

    private static void TestDistances(Graph graph) {
        Console.WriteLine("[5] Distances:");
        var pairs = new[] { (10, 20), (10, 30), (20, 30) };
        
        foreach (var (u, v) in pairs.Where(p => p.Item1 <= graph.VertexCount && p.Item2 <= graph.VertexCount)) {
            var distance = graph.GetDistance(u, v);
            var distStr = distance == -1 ? "Unreachable" : distance.ToString();
            Console.WriteLine($"    Distance({u}, {v}): {distStr}");
        }
    }

    private static void TestConnectedComponents(Graph graph) {
        Console.WriteLine("[6] Connected Components:");
        var components = graph.GetConnectedComponents();
        
        if (components.Count > 0) {
            Console.WriteLine($"    Count: {components.Count}");
            Console.WriteLine($"    Largest: {components.First().Count} vertices");
            Console.WriteLine($"    Smallest: {components.Last().Count} vertices");
        } else {
            Console.WriteLine("    No components found");
        }
    }

    private static void TestDiameter(Graph graph) {
        Console.WriteLine("[7] Diameter:");
        var stopwatch = Stopwatch.StartNew();
        var diameter = graph.GetApproximateDiameter();
        stopwatch.Stop();
        Console.WriteLine($"    Approximate: {diameter} (took {stopwatch.ElapsedMilliseconds} ms)");
    }

    // ========================= PERFORMANCE MEASUREMENTS =========================

    private static string MeasureSearchAlgorithm(
        Graph graph, 
        Action<int, Dictionary<int, int?>, Dictionary<int, int>> searchFunc, 
        int runs) {
        
        if (graph.VertexCount == 0) return "N/A";
        
        var parents = new Dictionary<int, int?>(graph.VertexCount);
        var levels = new Dictionary<int, int>(graph.VertexCount);
        var random = new Random(42);
        var stopwatch = new Stopwatch();
        var totalTicks = 0L;
        
        for (var i = 0; i < runs; i++) {
            var startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            searchFunc(startVertex, parents, levels);
            stopwatch.Stop();
            totalTicks += stopwatch.ElapsedTicks;
        }
        
        var avgMilliseconds = (totalTicks * 1000.0 / Stopwatch.Frequency) / runs;
        return $"{avgMilliseconds:F4} ms";
    }

    private static string MeasureDijkstraWithStrategy(string filePath, GraphLibrary.Algorithms.IDijkstraStrategy strategy, int runs) {
        var graph = Graph.FromFileWeighted(
            filePath,
            v => new AdjacencyList(v),
            strategy,
            GraphInputOptions.IsDirected);
        if (graph.VertexCount == 0) return "N/A";
        
        var random = new Random(42);
        var stopwatch = new Stopwatch();
        var totalTicks = 0L;
        
        for (var i = 0; i < runs; i++) {
            var startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            graph.Dijkstra(startVertex);
            stopwatch.Stop();
            totalTicks += stopwatch.ElapsedTicks;
        }
        
        var avgMilliseconds = (totalTicks * 1000.0 / Stopwatch.Frequency) / runs;
        return $"{avgMilliseconds:F4} ms";
    }

    private static double MeasureBellmanFord(Graph graph, int targetVertex, int runs) {
        if (graph.VertexCount == 0) return 0;

        var stopwatch = new Stopwatch();
        var totalTicks = 0L;

        for (var i = 0; i < runs; i++) {
            stopwatch.Restart();
            graph.BellmanFordToTarget(targetVertex);
            stopwatch.Stop();
            totalTicks += stopwatch.ElapsedTicks;
        }

        return (totalTicks / (double)Stopwatch.Frequency) / runs;
    }

    private static double MeasureDijkstraAtTarget(Graph graph, int targetVertex, int runs) {
        if (graph.VertexCount == 0) return 0;

        var stopwatch = new Stopwatch();
        var totalTicks = 0L;

        for (var i = 0; i < runs; i++) {
            stopwatch.Restart();
            graph.Dijkstra(targetVertex);
            stopwatch.Stop();
            totalTicks += stopwatch.ElapsedTicks;
        }

        return (totalTicks / (double)Stopwatch.Frequency) / runs;
    }

    // ========================= UTILITY METHODS =========================

    private static List<string> GetGraphFiles(string pattern) {
        var files = new List<string>();
        for (var i = 1; i <= MaxGraphFiles; i++) {
            var path = Path.Combine(BasePath, string.Format(pattern, i));
            if (File.Exists(path))
                files.Add(path);
        }
        return files;
    }

    private static (Graph graph, long memoryUsed) LoadGraphWithMemoryTracking(
        string filePath,
        Func<int, IGraphRepresentation> representationFactory,
        Func<string, Func<int, IGraphRepresentation>, IDijkstraStrategy?, bool, Graph> loadFunction,
        bool isDirected) {
        
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        
        var memoryBefore = GC.GetTotalMemory(true);
        var graph = loadFunction(filePath, representationFactory, null, isDirected);
        var memoryAfter = GC.GetTotalMemory(true);
        
        return (graph, memoryAfter - memoryBefore);
    }

    private static List<int> ReconstructPath(int start, int end, Dictionary<int, int?> parents) {
        var path = new List<int>();
        int? current = end;
        
        while (current.HasValue && parents.ContainsKey(current.Value)) {
            path.Add(current.Value);
            if (current.Value == start) break;
            current = parents[current.Value];
        }

        if (current != start) return [];
        
        if (path.Count > 0 && path[^1] != start) 
            path.Add(start);
        
        path.Reverse();
        return path;
    }

    private static List<int> ReconstructPathToTarget(int source, int target, Dictionary<int, int?> parents) {
        if (source == target)
            return new List<int> { source };

        var reversedPath = ReconstructPath(target, source, parents);
        if (reversedPath.Count == 0)
            return [];

        reversedPath.Reverse();
        return reversedPath;
    }

    private static string FormatDistance(double distance) =>
        double.IsPositiveInfinity(distance) ? "Inalcançável" : $"{distance:F4}";

    private static string FormatPath(List<int> path, int maxDisplay = 5) {
        if (path.Count == 0) return "N/A";
        
        var display = path.Take(maxDisplay).Select(v => v.ToString());
        var pathStr = string.Join(" → ", display);
        
        return path.Count > maxDisplay ? $"{pathStr}..." : pathStr;
    }

    // ========================= FORMATTING HELPERS =========================

    private static void PrintHeader() {
        Console.WriteLine(new string('#', 80));
        Console.WriteLine("### GRAPH ANALYSIS TEST SUITE");
        Console.WriteLine($"### Part 1 (Unweighted): {(FeatureToggles.RunPart1 ? "ENABLED" : "DISABLED")}");
        Console.WriteLine($"### Part 2 (Weighted): {(FeatureToggles.RunPart2 ? "ENABLED" : "DISABLED")}");
        Console.WriteLine($"### Directed Input: {(GraphInputOptions.IsDirected ? "YES" : "NO")}");
        Console.WriteLine(new string('#', 80));
    }

    private static void PrintFooter() {
        Console.WriteLine("\n" + new string('#', 80));
        Console.WriteLine("### ANÁLISES CONCLUÍDAS");
        Console.WriteLine(new string('#', 80));
    }

    private static void PrintSectionHeader(string title) {
        Console.WriteLine("\n" + new string('#', 80));
        Console.WriteLine($"### {title}");
        Console.WriteLine(new string('#', 80));
    }

    private static void PrintFileHeader(string filePath, string representation, string part) {
        Console.WriteLine($"\n{new string('=', 80)}");
        Console.WriteLine($"[{part}] {Path.GetFileName(filePath)} | {representation}");
        Console.WriteLine(new string('=', 80));
    }

    private static void PrintTestResult(string testName, string result) {
        Console.WriteLine($"[{testName}]: {result}");
    }

    private static void PrintError(string message) {
        Console.WriteLine($"\n[ERRO] {message}");
    }
}
