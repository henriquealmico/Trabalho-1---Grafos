# GraphLib - Graph Analysis Library

A comprehensive C# library for graph analysis and algorithm implementation, featuring flexible architecture with multiple representation strategies and optimized algorithms.

## 🏗️ Architecture Overview

### Design Patterns

The library implements several software design patterns to ensure flexibility, maintainability, and extensibility:

#### 1. **Strategy Pattern**
- **Graph Representations**: `IGraphRepresentation` interface allows swapping between different graph storage mechanisms (Adjacency List, Adjacency Matrix)
- **Dijkstra Algorithms**: `IDijkstraStrategy` interface enables different priority queue implementations (Heap-based O((E+V)logV), Array-based O(V²))

#### 2. **Dependency Injection**
- Graph representations are injected via factory functions in the constructor
- Dijkstra strategies are injected through constructor parameters with sensible defaults
- Enables testability and runtime algorithm selection

#### 3. **Factory Method Pattern**
- Static factory methods `FromFileUnweighted()` and `FromFileWeighted()` create graph instances from files
- Accepts representation factories as parameters for flexible instantiation

#### 4. **Lazy Initialization**
- Degree cache (`_allDegreesCache`) computed only when needed
- Dijkstra distances computed on-demand for discovered vertices

## 📁 Project Structure

```
GraphLib/
├── src/
│   ├── Graph.cs                          # Core graph class with algorithms
│   ├── Algorithms/
│   │   ├── IDijkstraStrategy.cs          # Strategy interface for Dijkstra
│   │   ├── DijkstraHeapStrategy.cs       # Heap-based implementation O((E+V)logV)
│   │   ├── DijkstraArrayStrategy.cs      # Array-based implementation O(V²)
│   │   └── BellmanFordResult.cs          # Result record + enums for Bellman-Ford
│   └── Representations/
│       ├── IGraphRepresentation.cs       # Strategy interface for graph storage
│       ├── AdjacencyList.cs              # Dictionary-based adjacency list
│       └── AdjacencyMatrix.cs            # 2D array-based adjacency matrix
├── Program.cs                            # Test suite and benchmarking
└── GraphLib.csproj                       # .NET 8.0 project configuration
```

## 🎯 Core Components

### Graph Class (`Graph.cs`)

The main class that encapsulates graph data and provides algorithm implementations.

**Key Features:**
- Vertex indexing: 1-based (vertices numbered from 1 to N)
- Supports weighted/unweighted and directed/undirected graphs (runtime flag)
- Automatic detection of negative weights
- Bellman-Ford with negative-cycle detection and target-oriented trees
- Vertex name mapping support for collaboration networks

**Private Fields:**
- `_representation`: Injected graph representation strategy
- `_dijkstraStrategy`: Injected Dijkstra algorithm strategy
- `_allDegreesCache`: Cached degree calculations
- `_vertexStringToInt`, `_vertexIntToString`: Vertex name mappings

### Graph Representations

#### `IGraphRepresentation`
Interface defining the contract for graph storage mechanisms.

**Methods:**
- `AddEdge(int u, int v, double weight, bool isDirected)`: Add weighted edge honoring graph direction
- `GetNeighbors(int v)`: Return enumerable of (neighbor, weight) tuples
- `VertexCount`, `EdgeCount`: Graph metrics

#### `AdjacencyList`
Dictionary-based representation optimized for sparse graphs.

**Implementation:**
- Uses `Dictionary<int, Dictionary<int, double>>`
- Space complexity: O(V + E)
- Neighbor retrieval: O(degree(v))
- Best for sparse graphs (E << V²)

#### `AdjacencyMatrix`
2D array representation optimized for dense graphs.

**Implementation:**
- Uses `double[,]` array
- Space complexity: O(V²)
- Neighbor retrieval: O(V)
- Best for dense graphs or when checking edge existence frequently

### Dijkstra Strategies

#### `IDijkstraStrategy`
Interface defining the contract for Dijkstra algorithm implementations.

**Methods:**
- `Initialize(int vertexCount)`: Prepare data structures
- `AddVertex(int vertex, double distance)`: Add/update vertex distance
- `TryGetNext(out int vertex, out double distance)`: Extract minimum
- `IsEmpty()`: Check if more vertices to process

#### `DijkstraHeapStrategy`
Priority queue-based implementation using .NET's `PriorityQueue<T, TPriority>`.

**Characteristics:**
- Time complexity: O((E + V) log V)
- Uses `HashSet<int>` to track visited vertices
- Optimal for sparse graphs
- Default strategy when none specified

#### `DijkstraArrayStrategy`
Array-based implementation with linear search for minimum.

**Characteristics:**
- Time complexity: O(V²)
- Uses `Dictionary<int, double>` for distances
- Better for dense graphs where E ≈ V²
- Simpler implementation with no heap overhead

## 🔍 Implemented Algorithms

### Search Algorithms

#### Breadth-First Search (BFS)
- **Method**: `BreadthFirstSearch(int startVertex)`
- **Returns**: Parent tree and level/distance from start
- **Complexity**: O(V + E)
- **Use Case**: Shortest path in unweighted graphs, level-order traversal

#### Depth-First Search (DFS)
- **Method**: `DepthFirstSearch(int startVertex)`
- **Returns**: Parent tree and discovery levels
- **Complexity**: O(V + E)
- **Use Case**: Connectivity, topological sorting, cycle detection

### Shortest Path Algorithms

#### Dijkstra's Algorithm
- **Method**: `Dijkstra(int startVertex)`
- **Returns**: Distance dictionary and parent tree
- **Complexity**: Depends on injected strategy
  - Heap: O((E + V) log V)
  - Array: O(V²)
- **Constraint**: No negative weights (throws exception if detected)

#### Bellman-Ford (Target-Oriented)
- **Methods**: `BellmanFordFromSource(int startVertex)` and `BellmanFordToTarget(int targetVertex)`
- **Optimizations**: SPFA-style queue with early exit plus enqueue-count negative cycle detection
- **Returns**: Distances, parent tree (pointing toward the target), and `HasNegativeCycle` flag
- **Use Case**: Directed graphs with negative weights or when computing distances from all vertices to a single target (graphs inverted internally)

### Graph Analysis

#### Distance Calculation
- **Method**: `GetDistance(int u, int v)`
- **Algorithm**: BFS-based shortest path
- **Returns**: Distance or -1 if unreachable

#### Diameter Estimation
- **Method**: `GetApproximateDiameter()`
- **Algorithm**: Two-sweep BFS approximation
- **Returns**: Approximate graph diameter

#### Connected Components
- **Method**: `GetConnectedComponents()`
- **Algorithm**: BFS-based component detection
- **Returns**: List of components sorted by size (descending)

#### Degree Metrics
- **Method**: `GetDegreeMetrics()`
- **Returns**: Dictionary with min, max, average, and median degrees
- **Optimization**: Results cached for subsequent calls

## 🚀 Usage Examples

### Creating a Graph with Adjacency List

```csharp
// Unweighted graph
var graph = Graph.FromFileUnweighted(
    "graph.txt",
    vertexCount => new AdjacencyList(vertexCount)
);

// Weighted graph with custom Dijkstra strategy
var graph = Graph.FromFileWeighted(
    "weighted_graph.txt",
    vertexCount => new AdjacencyList(vertexCount),
   new DijkstraArrayStrategy(),  // Optional: defaults to HeapStrategy
   isDirected: true              // Optional: defaults to false
);
```

#### Enabling Directed Parsing via CLI

```bash
dotnet run -- --directed     # treat input graphs as directed
dotnet run -- --undirected   # override back to undirected (default)
```

### Creating a Graph with Adjacency Matrix

```csharp
var graph = Graph.FromFileUnweighted(
    "graph.txt",
    vertexCount => new AdjacencyMatrix(vertexCount)
);
```

### Running Algorithms

```csharp
// BFS from vertex 1
var (parents, levels) = graph.BreadthFirstSearch(1);

// Dijkstra shortest paths from vertex 10
var (distances, pathParents) = graph.Dijkstra(10);

// Get distance between two vertices
int distance = graph.GetDistance(1, 50);

// Find connected components
var components = graph.GetConnectedComponents();

// Get degree statistics
var metrics = graph.GetDegreeMetrics();
double avgDegree = metrics["avg_degree"];
```

### Working with Named Vertices

```csharp
graph.LoadVertexNames("vertex_names.txt");

int vertexId = graph.GetVertexId("Edsger W. Dijkstra");
var (distances, parents) = graph.Dijkstra(vertexId);

string vertexName = graph.GetVertexName(42);
```

## 📊 File Formats

### Unweighted Graph Format
```
<number_of_vertices>
<vertex_u> <vertex_v>
<vertex_u> <vertex_v>
...
```

### Weighted Graph Format
```
<number_of_vertices>
<vertex_u> <vertex_v> <weight>
<vertex_u> <vertex_v> <weight>
...
```

### Vertex Names Format
```
<vertex_id>,<vertex_name>
<vertex_id>,<vertex_name>
...
```

## 📚 Part 3 Case Studies

The new study pipeline automates the following for every `grafo_W_*.txt` input:

1. **Bellman-Ford distances** from vertices 10, 20, 30 to vertex 100 (single run with inverted edges), along with the shortest-path tree and negative-cycle detection.
2. **Average Bellman-Ford runtime** over 10 executions (excluding disk I/O).
3. **Dijkstra comparison** on inverted graphs whenever there are no negative weights, producing distance and runtime comparison tables.

Results are printed in markdown-friendly tables under the `PARTE 3` section when the `RunPart3` toggle is enabled.

## ⚙️ Configuration

The test suite in `Program.cs` includes feature toggles:

```csharp
private static class FeatureToggles {
    public const bool RunPart1 = false;                    // Unweighted graphs
    public const bool RunPart1AdjacencyList = false;
    public const bool RunPart1AdjacencyMatrix = false;
    
    public const bool RunPart2 = true;                     // Weighted graphs
    public const bool RunPart2WeightedGraphs = true;
   public const bool RunPart2CollaborationNetwork = true;

   public const bool RunPart3 = true;                     // Directed / Bellman-Ford studies
   public const bool RunPart3CaseStudies = true;
}
```

Pass `--directed` or `--undirected` when running `dotnet run` to set the global graph orientation consumed by all loaders.

## 🎯 Design Principles

### SOLID Principles Applied

1. **Single Responsibility Principle**
   - Each class has one reason to change
   - `Graph` manages algorithms, representations handle storage
   - Strategies encapsulate specific algorithm implementations

2. **Open/Closed Principle**
   - Extensible through interfaces without modifying existing code
   - New representations: implement `IGraphRepresentation`
   - New Dijkstra strategies: implement `IDijkstraStrategy`

3. **Liskov Substitution Principle**
   - Any `IGraphRepresentation` can replace another
   - Any `IDijkstraStrategy` can replace another
   - Behavior remains consistent

4. **Interface Segregation Principle**
   - Small, focused interfaces
   - Clients depend only on methods they use

5. **Dependency Inversion Principle**
   - Graph depends on abstractions (`IGraphRepresentation`, `IDijkstraStrategy`)
   - High-level algorithm logic decoupled from low-level data structures

### Additional Patterns

- **Encapsulation**: Internal implementation details hidden
- **Immutability**: Read-only properties where appropriate
- **Fail-Fast**: Immediate validation and exception throwing
- **Caching**: Performance optimization through lazy computation

## 🔧 Performance Optimizations

### Dijkstra Algorithm
- Lazy distance initialization (only visited vertices)
- Single `TryGetValue` call per neighbor with branching
- Pre-allocated data structures with capacity hints

### Representation-Specific
- **AdjacencyList**: Direct dictionary enumeration (no LINQ overhead)
- **AdjacencyMatrix**: Yield-based iteration for memory efficiency

### Strategy Selection
- **Heap Strategy**: Sparse graphs, E << V²
- **Array Strategy**: Dense graphs, E ≈ V²

## 🛠️ Technology Stack

- **Language**: C# 12
- **Framework**: .NET 8.0
- **Data Structures**: 
  - `Dictionary<TKey, TValue>`
  - `HashSet<T>`
  - `PriorityQueue<TElement, TPriority>`
  - `Queue<T>`
  - `Stack<T>`

## 📈 Complexity Analysis

| Operation | Adjacency List | Adjacency Matrix |
|-----------|---------------|------------------|
| Space | O(V + E) | O(V²) |
| Add Edge | O(1) | O(1) |
| Get Neighbors | O(degree(v)) | O(V) |
| Check Edge | O(degree(v)) | O(1) |
| BFS/DFS | O(V + E) | O(V²) |
| Dijkstra (Heap) | O((E + V) log V) | O(V² log V) |
| Dijkstra (Array) | O(V² + E) | O(V²) |

## 🧪 Testing

The `Program.cs` file includes comprehensive test suites:

- Memory usage analysis
- Algorithm performance benchmarking (100 runs)
- Correctness verification
- Parent tree validation
- Distance calculations
- Component analysis
- Diameter computation

## 📝 Notes

- **Vertex Numbering**: 1-based indexing (1 to N)
- **Graph Mode**: Directed or undirected via runtime flag / CLI switch
- **Negative Weights**: Automatically skipped by Dijkstra; handled by Bellman-Ford with detection
- **Bellman-Ford**: Computes all-to-target distances on inverted graphs and reports negative cycles
- **Thread Safety**: Not thread-safe (designed for single-threaded use)

## 🤝 Contributing

When extending the library:

1. Implement appropriate strategy interfaces
2. Follow existing naming conventions
3. Maintain O-notation complexity in comments
4. Add corresponding tests in `Program.cs`
5. Update this README with new features

## 📄 License

This project is part of a graph theory assignment and is intended for educational purposes.
