using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Benchmarks.Data;

internal sealed class BenchmarkVertex : IVertex, IPathfindingVertex
{
    public bool IsObstacle { get; set; }

    public IVertexCost Cost { get; set; }

    public Coordinate Position { get; set; }

    public BenchmarkVertex[] Neighbors { get; private set; }
        = Array.Empty<BenchmarkVertex>();

    IReadOnlyCollection<IPathfindingVertex> IPathfindingVertex.Neighbors => Neighbors;

    IReadOnlyCollection<IVertex> IVertex.Neighbors
    {
        get => Neighbors;
        set => Neighbors = value.Cast<BenchmarkVertex>().ToArray();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override bool Equals(object obj)
    {
        return obj is BenchmarkVertex vert && Equals(vert);
    }

    public bool Equals(IVertex other)
    {
        throw new System.NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode()
    {
        return Position.GetHashCode();
    }
}