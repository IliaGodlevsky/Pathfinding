using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class LeeAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
    : BreadthFirstAlgorithm<Queue<IPathfindingVertex>>(pathfindingRange)
{
    protected override void DropState()
    {
        base.DropState();
        storage.Clear();
    }

    protected override void MoveNextVertex()
    {
        CurrentVertex = storage.DequeueOrThrowDeadEndVertexException();
    }

    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        storage.Enqueue(vertex);
        base.RelaxVertex(vertex);
    }
}
