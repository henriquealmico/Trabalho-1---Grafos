using System.Diagnostics;
using GraphLibrary;
using GraphLibrary.Representations;

public class Program {
    public static void Main(string[] args) {
        if (args.Length == 0) {
            AnalyzeGraphFile("C:/Users/halmi/Downloads/Trabalho 1 - Grafos/test_cases/grafo_1.txt");
            return;
        }

        var path = args[0];
        var filesToProcess = new List<string>();

        if (File.Exists(path))
            filesToProcess.Add(path);
        
        else if (Directory.Exists(path))
            filesToProcess.AddRange(Directory.GetFiles(path, "*.txt"));
        
        else {
            Console.WriteLine($"Error: Path not found '{path}'");
            return;
        }

        foreach (var filePath in filesToProcess)
            AnalyzeGraphFile(filePath);
    }

    public static void AnalyzeGraphFile(string filePath) {
        Console.WriteLine(new string('=', 80));
        Console.WriteLine($"ANALYZING GRAPH FILE: {Path.GetFileName(filePath)}");
        Console.WriteLine(new string('=', 80));

        try {
            GC.Collect();
            var memoryBeforeList = GC.GetTotalMemory(true);
            var graphList = Graph.FromFile(filePath, v => new AdjacencyList(v));
            var memoryAfterList = GC.GetTotalMemory(true);
            
            RunCaseStudies(graphList, "Adjacency List", memoryAfterList - memoryBeforeList);

            // --- Adjacency Matrix Analysis ---
            GC.Collect();
            var memoryBeforeMatrix = GC.GetTotalMemory(true);
            var graphMatrix = Graph.FromFile(filePath, v => new AdjacencyMatrix(v));
            var memoryAfterMatrix = GC.GetTotalMemory(true);

            RunCaseStudies(graphMatrix, "Adjacency Matrix", memoryAfterMatrix - memoryBeforeMatrix);
        }
        catch (Exception ex) {
            Console.WriteLine($"Failed to process file {filePath}. Error: {ex.Message}");
        }
    }

    public static void RunCaseStudies(Graph graph, string representationType, long memoryBytes) {
        Console.WriteLine($"\n--- Case Study Results for: {representationType} ---");
        
        // --- Question 1: Memory Usage ---
        Console.WriteLine($"[1] Memory Usage: {memoryBytes / (1024.0 * 1024.0):F3} MB");

        // --- Question 2 & 3: BFS and DFS Performance ---
        Console.WriteLine("[2] BFS Average Time (100 runs): " + TimeSearchAlgorithm(graph, graph.BreadthFirstSearch, 100));
        Console.WriteLine("[3] DFS Average Time (100 runs): " + TimeSearchAlgorithm(graph, graph.DepthFirstSearch, 100));

        var verticesToTest = new[] { 10, 20, 30 };
        var startVertices = new[] { 1, 2, 3 };

        // --- Question 4: Parent Vertices ---
        Console.WriteLine("[4] Parent Vertices:");
        foreach (var startNode in startVertices)
        {
            if (startNode > graph.VertexCount) continue;
            var (parentsBfs, _) = graph.BreadthFirstSearch(startNode);
            var (parentsDfs, _) = graph.DepthFirstSearch(startNode);
            foreach (var targetNode in verticesToTest)
            {
                if (targetNode > graph.VertexCount) continue;
                string bfsParent = parentsBfs.ContainsKey(targetNode) && parentsBfs[targetNode].HasValue ? parentsBfs[targetNode].Value.ToString() : "N/A";
                string dfsParent = parentsDfs.ContainsKey(targetNode) && parentsDfs[targetNode].HasValue ? parentsDfs[targetNode].Value.ToString() : "N/A";
                Console.WriteLine($"    - Start {startNode} -> Target {targetNode}: BFS Parent={bfsParent}, DFS Parent={dfsParent}");
            }
        }
        
        // --- Question 5: Distances ---
        var pairsToTest = new[] { (10, 20), (10, 30), (20, 30) };
        Console.WriteLine("[5] Distances between pairs:");
        foreach (var pair in pairsToTest)
        {
            if (pair.Item1 > graph.VertexCount || pair.Item2 > graph.VertexCount) continue;
            int distance = graph.GetDistance(pair.Item1, pair.Item2);
            Console.WriteLine($"    - Distance({pair.Item1}, {pair.Item2}): {(distance == -1 ? "Unreachable" : distance)}");
        }

        // --- Question 6: Connected Components ---
        Console.WriteLine("[6] Connected Components:");
        var components = graph.GetConnectedComponents();
        if (components.Any())
        {
            Console.WriteLine($"    - Number of components: {components.Count}");
            Console.WriteLine($"    - Largest component size: {components.First().Count}");
            Console.WriteLine($"    - Smallest component size: {components.Last().Count}");
        }
        else
        {
            Console.WriteLine("    - No components found (empty graph).");
        }
        
        // --- Question 7: Diameter ---
        Console.WriteLine("[7] Diameter:");
        var stopwatch = Stopwatch.StartNew();
        int approxDiameter = graph.GetApproximateDiameter();
        stopwatch.Stop();
        Console.WriteLine($"    - Approximate Diameter (2-BFS): {approxDiameter} (took {stopwatch.ElapsedMilliseconds} ms)");
        
        if (graph.VertexCount < 2000)
        {
            stopwatch.Restart();
            int exactDiameter = graph.GetDiameter();
            stopwatch.Stop();
            Console.WriteLine($"    - Exact Diameter (N-BFS): {exactDiameter} (took {stopwatch.ElapsedMilliseconds} ms)");
        }
        else
        {
             Console.WriteLine($"    - Exact Diameter (N-BFS): Skipped (graph has {graph.VertexCount} vertices > 2000)");
        }
    }

    private static string TimeSearchAlgorithm(Graph graph, Func<int, (Dictionary<int, int?>, Dictionary<int, int>)> searchFunc, int runs) {
        if (graph.VertexCount == 0) return "N/A (empty graph)";
        
        var random = new Random();
        var totalTime = TimeSpan.Zero;
        var stopwatch = new Stopwatch();

        for (int i = 0; i < runs; i++) {
            int startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            searchFunc(startVertex);
            stopwatch.Stop();
            totalTime += stopwatch.Elapsed;
        }

        return $"{(totalTime.TotalMilliseconds / runs):F4} ms";
    }
}