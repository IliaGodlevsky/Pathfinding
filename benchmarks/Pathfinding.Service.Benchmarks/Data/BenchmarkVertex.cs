using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Runtime.CompilerServices;

namespace Pathfinding.Service.Benchmarks.Data;

internal sealed class BenchmarkVertex : IVertex, IPathfindingVertex
{
    public bool IsObstacle { get; set; }

    public IVertexCost Cost { get; set; }

    public Coordinate Position { get; set; }

    public BenchmarkVertex[] Neighbors { get; private set; } = [];

    IReadOnlyCollection<IPathfindingVertex> IPathfindingVertex.Neighbors => Neighbors;

    IReadOnlyCollection<IVertex> IVertex.Neighbors
    {
        get => Neighbors;
        set => Neighbors = [.. value.Cast<BenchmarkVertex>()];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
        return obj is BenchmarkVertex vert && Equals(vert);
    }

    public bool Equals(IVertex other)
    {
        return other.IsEqual(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}