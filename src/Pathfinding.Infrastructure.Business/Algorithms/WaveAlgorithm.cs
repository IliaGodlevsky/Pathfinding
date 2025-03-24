using Pathfinding.Service.Interface;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class WaveAlgorithm<TStorage>(IEnumerable<IPathfindingVertex> pathfindingRange)
    : PathfindingAlgorithm<TStorage>(pathfindingRange)
    where TStorage : new()
{
    protected abstract void RelaxVertex(IPathfindingVertex vertex);

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        VisitCurrentVertex();
    }

    protected override void VisitCurrentVertex()
    {
        visited.Add(CurrentVertex);
    }

    protected virtual void RelaxNeighbours(IReadOnlyCollection<IPathfindingVertex> vertices)
    {
        vertices.ForEach(RelaxVertex);
    }

    protected override void InspectCurrentVertex()
    {
        var neighbours = GetUnvisitedNeighbours(CurrentVertex);
        RaiseVertexProcessed(CurrentVertex, neighbours);
        RelaxNeighbours(neighbours);
    }
}