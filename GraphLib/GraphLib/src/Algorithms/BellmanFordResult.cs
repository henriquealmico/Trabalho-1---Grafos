using System.Collections.Generic;

namespace GraphLibrary.Algorithms;

public enum BellmanFordResultMode {
    FromSource,
    ToTarget
}

public readonly record struct BellmanFordResult(
    Dictionary<int, double> Distances,
    Dictionary<int, int?> Parents,
    bool HasNegativeCycle,
    int ReferenceVertex,
    BellmanFordResultMode Mode);
