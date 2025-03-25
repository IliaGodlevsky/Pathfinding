using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class PathfindingAlgorithm<TStorage>(IEnumerable<IPathfindingVertex> pathfindingRange)
    : PathfindingProcess(pathfindingRange)
    where TStorage : new()
{
    protected readonly HashSet<IPathfindingVertex> visited = [];
    protected readonly Dictionary<Coordinate, IPathfindingVertex> traces = [];
    protected readonly TStorage storage = new();

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
        return new GraphPath(traces.ToFrozenDictionary(),
            CurrentRange.Target);
    }

    protected override void DropState()
    {
        visited.Clear();
        traces.Clear();
    }

    protected virtual IReadOnlyCollection<IPathfindingVertex> GetUnvisitedNeighbours(
        IPathfindingVertex vertex)
    {
        return vertex.Neighbors
            .Where(v => !v.IsObstacle && !visited.Contains(v))
            .ToArray();
    }
}