using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class PathfindingAlgorithm<TStorage>(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : PathfindingProcess(pathfindingRange)
    where TStorage : new()
{
    protected readonly HashSet<IPathfindingVertex> Visited = [];
    protected readonly Dictionary<Coordinate, IPathfindingVertex> Traces = [];
    protected readonly TStorage Storage = new();

    protected SubRange CurrentRange { get; set; } = SubRange.Default;

    protected IPathfindingVertex CurrentVertex { get; set; } = NullPathfindingVertex.Interface;

    protected override bool IsDestination()
    {
        return CurrentVertex.Equals(CurrentRange.Target);
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        CurrentRange = range;
        CurrentVertex = CurrentRange.Source;
    }

    protected override IGraphPath GetSubPath()
    {
        return new GraphPath(Traces.ToFrozenDictionary(),
            CurrentRange.Target);
    }

    protected override void DropState()
    {
        Visited.Clear();
        Traces.Clear();
    }

    protected virtual IReadOnlyCollection<IPathfindingVertex> GetUnvisitedNeighbours(
        IPathfindingVertex vertex)
    {
        return vertex.Neighbors
            .Where(v => !v.IsObstacle && !Visited.Contains(v))
            .ToArray();
    }
}