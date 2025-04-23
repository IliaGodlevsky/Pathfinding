using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public abstract class DepthAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : PathfindingAlgorithm<Stack<IPathfindingVertex>>(pathfindingRange)
{
    private IPathfindingVertex PreviousVertex { get; set; } = NullPathfindingVertex.Instance;

    protected abstract IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors);

    protected override void MoveNextVertex()
    {
        var neighbours = GetUnvisitedNeighbours(CurrentVertex);
        RaiseVertexProcessed(CurrentVertex, neighbours);
        CurrentVertex = GetVertex(neighbours);
    }

    protected override void PrepareForSubPathfinding(SubRange range)
    {
        base.PrepareForSubPathfinding(range);
        Visited.Add(CurrentVertex);
        Storage.Push(CurrentVertex);
    }

    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
    }

    protected override void VisitCurrentVertex()
    {
        if (CurrentVertex.Neighbors.Count == 0)
        {
            CurrentVertex = Storage.PopOrThrowDeadEndVertexException();
        }
        else
        {
            Visited.Add(CurrentVertex);
            Storage.Push(CurrentVertex);
            Traces[CurrentVertex.Position] = PreviousVertex;
        }
    }

    protected override void InspectCurrentVertex()
    {
        PreviousVertex = CurrentVertex;
    }
}