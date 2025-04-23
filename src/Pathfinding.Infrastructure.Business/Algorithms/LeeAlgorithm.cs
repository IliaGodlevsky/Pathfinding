using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class LeeAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : BreadthFirstAlgorithm<Queue<IPathfindingVertex>>(pathfindingRange)
{
    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
    }

    protected override void MoveNextVertex()
    {
        CurrentVertex = Storage.DequeueOrThrowDeadEndVertexException();
    }

    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        Storage.Enqueue(vertex);
        base.RelaxVertex(vertex);
    }
}
