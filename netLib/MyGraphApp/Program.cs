using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GraphLibrary;
using GraphLibrary.Representations;

public class Program
{
    // IMPORTANT: Update this path to the folder containing your graph files.
    private const string basePath = "C:/Users/halmi/Downloads/Trabalho 1 - Grafos/test_cases/";

    public static void Main(string[] args)
    {
        var filePaths = new List<string>();
        for (int i = 1; i <= 6; i++)
        {
            string path = Path.Combine(basePath, $"grafo_{i}.txt");
            if (File.Exists(path))
            {
                filePaths.Add(path);
            }
            else
            {
                Console.WriteLine($"Warning: File not found and will be skipped: {path}");
            }
        }
        
        // --- PHASE 1: Run all analyses using Adjacency List ---
        Console.WriteLine(new string('#', 80));
        Console.WriteLine("### STARTING ANALYSIS - PHASE 1: ADJACENCY LIST");
        Console.WriteLine(new string('#', 80));
        foreach (var filePath in filePaths)
        {
            AnalyzeWithAdjacencyList(filePath);
        }

        // --- PHASE 2: Run all analyses using Adjacency Matrix ---
        Console.WriteLine("\n\n" + new string('#', 80));
        Console.WriteLine("### STARTING ANALYSIS - PHASE 2: ADJACENCY MATRIX");
        Console.WriteLine(new string('#', 80));
        foreach (var filePath in filePaths)
        {
            AnalyzeWithAdjacencyMatrix(filePath);
        }
        
        Console.WriteLine("\n\n" + new string('#', 80));
        Console.WriteLine("### All analyses complete.");
        Console.WriteLine(new string('#', 80));
    }

    public static void AnalyzeWithAdjacencyList(string filePath)
    {
        Console.WriteLine($"\n{new string('=', 80)}\nANALYZING: {Path.GetFileName(filePath)}");
        try
        {
            GC.Collect();
            long memoryBefore = GC.GetTotalMemory(true);
            var graph = Graph.FromFile(filePath, v => new AdjacencyList(v));
            long memoryAfter = GC.GetTotalMemory(true);
            
            RunCaseStudies(graph, "Adjacency List", memoryAfter - memoryBefore);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to process file {filePath}. Reason: {ex.Message}");
        }
    }

    public static void AnalyzeWithAdjacencyMatrix(string filePath)
    {
        Console.WriteLine($"\n{new string('=', 80)}\nANALYZING: {Path.GetFileName(filePath)}");
        try
        {
            GC.Collect();
            long memoryBefore = GC.GetTotalMemory(true);
            var graph = Graph.FromFile(filePath, v => new AdjacencyMatrix(v));
            long memoryAfter = GC.GetTotalMemory(true);

            RunCaseStudies(graph, "Adjacency Matrix", memoryAfter - memoryBefore);
        }
        catch (OutOfMemoryException)
        {
            Console.WriteLine($"\n--- SKIPPING Adjacency Matrix for {Path.GetFileName(filePath)} ---");
            Console.WriteLine("      [REASON] OutOfMemoryException: The graph is too large to be represented by an Adjacency Matrix.");
            Console.WriteLine("      Continuing to the next file...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERROR] Failed to process file {filePath}. Reason: {ex.Message}");
        }
    }

    public static void RunCaseStudies(Graph graph, string representationType, long memoryBytes)
    {
        Console.WriteLine($"\n--- Case Study Results for: {representationType} ---");
        
        Console.WriteLine($"[1] Memory Usage: {memoryBytes / (1024.0 * 1024.0):F3} MB");
        
        // Using the optimized internal methods for performance timing
        Console.WriteLine("[2] BFS Average Time (100 runs): " + TimeSearchAlgorithm(graph, graph.BreadthFirstSearch, 100));
        Console.WriteLine("[3] DFS Average Time (100 runs): " + TimeSearchAlgorithm(graph, graph.DepthFirstSearch, 100));

        var verticesToTest = new[] { 10, 20, 30 };
        var startVertices = new[] { 1, 2, 3 };
        
        Console.WriteLine("[4] Parent Vertices:");
        foreach (var startNode in startVertices)
        {
            if (startNode > graph.VertexCount) continue;
            // Using the simple public methods for one-off calls
            var (parentsBfs, _) = graph.BreadthFirstSearch(startNode);
            var (parentsDfs, _) = graph.DepthFirstSearch(startNode);
            foreach (var targetNode in verticesToTest)
            {
                if (targetNode > graph.VertexCount) continue;
                string bfsParent = parentsBfs.GetValueOrDefault(targetNode)?.ToString() ?? "N/A";
                string dfsParent = parentsDfs.GetValueOrDefault(targetNode)?.ToString() ?? "N/A";
                Console.WriteLine($"    - Start {startNode} -> Target {targetNode}: BFS Parent={bfsParent}, DFS Parent={dfsParent}");
            }
        }
        
        var pairsToTest = new[] { (10, 20), (10, 30), (20, 30) };
        Console.WriteLine("[5] Distances between pairs:");
        foreach (var pair in pairsToTest)
        {
            if (pair.Item1 > graph.VertexCount || pair.Item2 > graph.VertexCount) continue;
            int distance = graph.GetDistance(pair.Item1, pair.Item2);
            Console.WriteLine($"    - Distance({pair.Item1}, {pair.Item2}): {(distance == -1 ? "Unreachable" : distance)}");
        }

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
             Console.WriteLine($"    - Exact Diameter (N-BFS): Skipped (graph has {graph.VertexCount} vertices, exceeds threshold of 2000)");
        }
    }

    // This method's signature now matches the new internal search methods
    private static string TimeSearchAlgorithm(Graph graph, Action<int, Dictionary<int, int?>, Dictionary<int, int>> searchFunc, int runs)
    {
        if (graph.VertexCount == 0) return "N/A (empty graph)";
        
        // Collections are created ONCE and reused to reduce memory pressure.
        var parents = new Dictionary<int, int?>(graph.VertexCount);
        var levels = new Dictionary<int, int>(graph.VertexCount);
        
        var random = new Random();
        var totalTime = TimeSpan.Zero;
        var stopwatch = new Stopwatch();

        for (int i = 0; i < runs; i++)
        {
            int startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            searchFunc(startVertex, parents, levels); // Pass collections for reuse
            stopwatch.Stop();
            totalTime += stopwatch.Elapsed;
        }
        return $"{(totalTime.TotalMilliseconds / runs):F4} ms";
    }
}