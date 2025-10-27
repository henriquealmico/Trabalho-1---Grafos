using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using GraphLibrary;
using GraphLibrary.Representations;

public class Program
{
    // IMPORTANTE: Atualize este caminho para a pasta com seus arquivos de grafo
    private const string basePath = "C:\\Users\\Augusto\\Documents\\GitHub";

    public static void Main(string[] args)
    {
        var filePaths = new List<string>();
        for (int i = 1; i <= 6; i++)
        {
            string path = Path.Combine(basePath, $"grafo_W_{i}.txt");
            if (File.Exists(path)) filePaths.Add(path);
        }
        
        // --- Execução dos Estudos de Caso (Parte 2) ---
        Console.WriteLine(new string('#', 80));
        Console.WriteLine("### INICIANDO ESTUDOS DE CASO - PARTE 2");
        Console.WriteLine(new string('#', 80));
        
        foreach (var filePath in filePaths)
        {
            RunDijkstraCaseStudies(filePath);
        }

        // --- Estudo de Caso da Rede de Colaboração ---
        RunCollaborationNetworkStudy();
    }

    /// <summary>
    /// Roda os estudos de caso 3.1 e 3.2 do PDF da Parte 2 
    /// </summary>
    public static void RunDijkstraCaseStudies(string filePath)
    {
        Console.WriteLine($"\n{new string('=', 80)}\nANALISANDO: {Path.GetFileName(filePath)}");
        Graph graph;
        try
        {
            // Carrega o grafo (usando AdjacencyList por performance)
            graph = Graph.FromFile(filePath, v => new AdjacencyList(v));

            if (graph.HasNegativeWeights)
            {
                Console.WriteLine("[AVISO] O grafo contém pesos negativos. O algoritmo de Dijkstra será pulado. ");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERRO] Falha ao carregar o grafo {filePath}. Razão: {ex.Message}");
            return;
        }

        // --- Estudo de Caso 3.1: Distância e Caminho Mínimo [cite: 94] ---
        Console.WriteLine("\n[Estudo de Caso 3.1: Distâncias e Caminhos Mínimos]");
        int startVertex = 10;
        var verticesToFind = new[] { 20, 30, 40, 50, 60 };
        
        // Usamos a implementação com Heap por ser a mais rápida
        var (distances, parents) = graph.Dijkstra(startVertex, useHeap: true); 
        
        Console.WriteLine($"| Vértice Inicial | Vértice Final | Distância | Caminho Mínimo (exemplos) |");
        Console.WriteLine($"|:--- |:--- |:--- |:--- |");
        foreach (var endVertex in verticesToFind)
        {
            if (endVertex > graph.VertexCount) continue;
            
            double dist = distances.GetValueOrDefault(endVertex, double.PositiveInfinity);
            string distStr = dist == double.PositiveInfinity ? "Inalcançável" : $"{dist:F2}";
            var path = ReconstructPath(startVertex, endVertex, parents);
            string pathStr = path.Any() ? string.Join(" -> ", path.Take(5)) + (path.Count > 5 ? "..." : "") : "N/A";
            
            Console.WriteLine($"| {startVertex, -15} | {endVertex, -13} | {distStr, -9} | {pathStr,-25} |");
        }

        // --- Estudo de Caso 3.2: Comparação de Tempo Dijkstra [cite: 95] ---
        Console.WriteLine("\n[Estudo de Caso 3.2: Tempo de Execução Dijkstra (k=100)]");
        int k = 100; // Número de execuções [cite: 96]
        
        string timeArray = TimeDijkstra(graph, useHeap: false, k);
        string timeHeap = TimeDijkstra(graph, useHeap: true, k);

        Console.WriteLine($"| Implementação | Tempo Médio (k={k}) |");
        Console.WriteLine($"|:--- |:--- |");
        Console.WriteLine($"| Dijkstra com Vetor (O(V^2)) | {timeArray, -18} |");
        Console.WriteLine($"| Dijkstra com Heap (O((E+V)logV)) | {timeHeap, -18} |");
    }

    /// <summary>
    /// Roda o estudo de caso da rede de colaboração 
    /// </summary>
    private static void RunCollaborationNetworkStudy()
    {
        Console.WriteLine($"\n{new string('=', 80)}\nANALISANDO: Rede de Colaboração ");
        string graphFile = Path.Combine(basePath, "collab_graph.txt"); // Arquivo de arestas
        string namesFile = Path.Combine(basePath, "collab_names.txt"); // Arquivo de mapeamento ID,Nome

        if (!File.Exists(graphFile) || !File.Exists(namesFile))
        {
            Console.WriteLine("[AVISO] Arquivos 'collab_graph.txt' ou 'collab_names.txt' não encontrados. Pulando estudo de caso.");
            return;
        }

        try
        {
            var graph = Graph.FromFile(graphFile, v => new AdjacencyList(v));
            graph.LoadVertexNames(namesFile);
            
            string startName = "Edsger W. Dijkstra";
            var targetNames = new[] 
            {
                "Alan M. Turing", 
                "J. B. Kruskal", 
                "Jon M. Kleinberg", 
                "Éva Tardos", 
                "Daniel R. Figueiredo"
            };

            int startId = graph.GetVertexId(startName);
            var (distances, parents) = graph.Dijkstra(startId, useHeap: true);

            Console.WriteLine($"\nResultados do Caminho Mínimo a partir de '{startName}':");
            Console.WriteLine($"| Pesquisador Destino | Distância (Proximidade) | Caminho (exemplo) |");
            Console.WriteLine($"|:--- |:--- |:--- |");

            foreach (var targetName in targetNames)
            {
                int endId = graph.GetVertexId(targetName);
                double dist = distances.GetValueOrDefault(endId, double.PositiveInfinity);
                string distStr = dist == double.PositiveInfinity ? "Inalcançável" : $"{dist:F4}";
                
                var pathIds = ReconstructPath(startId, endId, parents);
                var pathNames = pathIds.Select(id => graph.GetVertexName(id));
                string pathStr = pathIds.Any() ? string.Join(" -> ", pathNames.Take(4)) + "..." : "N/A";

                Console.WriteLine($"| {targetName,-20} | {distStr,-23} | {pathStr,-17} |");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n[ERRO] Falha ao processar a rede de colaboração. Razão: {ex.Message}");
        }
    }

    // --- Funções Auxiliares ---

    private static string TimeDijkstra(Graph graph, bool useHeap, int k)
    {
        if (graph.VertexCount == 0) return "N/A";
        
        var random = new Random();
        var totalTime = TimeSpan.Zero;
        var stopwatch = new Stopwatch();

        for (int i = 0; i < k; i++)
        {
            int startVertex = random.Next(1, graph.VertexCount + 1);
            stopwatch.Restart();
            graph.Dijkstra(startVertex, useHeap);
            stopwatch.Stop();
            totalTime += stopwatch.Elapsed;
        }
        return $"{(totalTime.TotalMilliseconds / k):F4} ms";
    }

    private static List<int> ReconstructPath(int start, int end, Dictionary<int, int?> parents)
    {
        var path = new List<int>();
        int? current = end;
        while (current.HasValue && current.Value != start)
        {
            path.Add(current.Value);
            current = parents.GetValueOrDefault(current.Value, null);
        }

        if (current.HasValue && current.Value == start)
        {
            path.Add(start);
            path.Reverse();
            return path;
        }
        return new List<int>(); // Retorna caminho vazio se não houver
    }
}