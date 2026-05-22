using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Algorithms;

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
