using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class BidirectLeeAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : BidirectBreadthFirstAlgorithm<Queue<IPathfindingVertex>>(pathfindingRange)
{
    protected override void DropState()
    {
        base.DropState();
        BackwardStorage.Clear();
        ForwardStorage.Clear();
    }

    protected override void RelaxForwardVertex(IPathfindingVertex vertex)
    {
        ForwardStorage.Enqueue(vertex);
        SetForwardIntersection(vertex);
        base.RelaxForwardVertex(vertex);
    }

    protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
    {
        BackwardStorage.Enqueue(vertex);
        SetBackwardIntersections(vertex);
        base.RelaxBackwardVertex(vertex);
    }

    protected override void MoveNextVertex()
    {
        var forward = ForwardStorage.DequeueOrThrowDeadEndVertexException();
        var backward = BackwardStorage.DequeueOrThrowDeadEndVertexException();
        Current = new(forward, backward);
    }

    protected override void VisitCurrentVertex()
    {
        ForwardVisited.Add(Current.Source);
        BackwardVisited.Add(Current.Target);
    }
}