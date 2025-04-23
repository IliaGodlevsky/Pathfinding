using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class BidirectPathfindingAlgorithm<TStorage>(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : PathfindingProcess(pathfindingRange)
    where TStorage : new()
{
    protected readonly TStorage ForwardStorage = new();
    protected readonly TStorage BackwardStorage = new();
    protected readonly HashSet<IPathfindingVertex> ForwardVisited = [];
    protected readonly HashSet<IPathfindingVertex> BackwardVisited = [];
    protected readonly Dictionary<Coordinate, IPathfindingVertex> ForwardTraces = [];
    protected readonly Dictionary<Coordinate, IPathfindingVertex> BackwardTraces = [];

    protected IPathfindingVertex Intersection { get; set; } = NullPathfindingVertex.Interface;

    protected SubRange Current { get; set; } = SubRange.Default;

    protected SubRange Range { get; set; } = SubRange.Default;

    protected override bool IsDestination()
    {
        return !Equals(Intersection, NullPathfindingVertex.Interface);
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        Range = range;
        Current = Range;
    }

    protected override IGraphPath GetSubPath()
    {
        return new BidirectGraphPath(
            ForwardTraces.ToFrozenDictionary(),
            BackwardTraces.ToFrozenDictionary(), Intersection);
    }

    protected virtual void SetForwardIntersection(IPathfindingVertex vertex)
    {
        if (Equals(Intersection, NullPathfindingVertex.Instance)
            && BackwardVisited.Contains(vertex))
        {
            Intersection = vertex;
        }
    }

    protected virtual void SetBackwardIntersections(IPathfindingVertex vertex)
    {
        if (Equals(Intersection, NullPathfindingVertex.Instance)
            && ForwardVisited.Contains(vertex))
        {
            Intersection = vertex;
        }
    }

    protected override void DropState()
    {
        ForwardVisited.Clear();
        BackwardVisited.Clear();
        ForwardTraces.Clear();
        BackwardTraces.Clear();
        Intersection = NullPathfindingVertex.Interface;
    }

    protected virtual IReadOnlyCollection<IPathfindingVertex> GetForwardUnvisitedNeighbours()
    {
        return GetUnvisitedNeighbours(Current.Source, ForwardVisited);
    }

    protected virtual IReadOnlyCollection<IPathfindingVertex> GetBackwardUnvisitedNeighbours()
    {
        return GetUnvisitedNeighbours(Current.Target, BackwardVisited);
    }

    private static IPathfindingVertex[] GetUnvisitedNeighbours(IPathfindingVertex vertex,
        HashSet<IPathfindingVertex> visited)
    {
        return [.. vertex.Neighbors.Where(v => !v.IsObstacle && !visited.Contains(v))];
    }
}
