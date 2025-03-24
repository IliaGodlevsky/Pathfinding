using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class BidirectRandomAlgorithm(IEnumerable<IPathfindingVertex> range)
    : BidirectBreadthFirstAlgorithm<List<IPathfindingVertex>>(range)
{
    private readonly Random random = new(range.Count()
        ^ range.Aggregate(0, (x, y) => x + y.Cost.CurrentCost));

    protected override void MoveNextVertex()
    {
        var forward = NullPathfindingVertex.Interface;
        var backward = NullPathfindingVertex.Interface;

        if (forwardStorage.Count > 0)
        {
            int index = random.Next(forwardStorage.Count);
            forward = forwardStorage[index];
            forwardStorage.RemoveAt(index);
        }
        if (backwardStorage.Count > 0)
        {
            int index = random.Next(backwardStorage.Count);
            backward = backwardStorage[index];
            backwardStorage.RemoveAt(index);
        }
        if (backward == NullPathfindingVertex.Interface
            || forward == NullPathfindingVertex.Interface)
        {
            throw new DeadendVertexException();
        }
        Current = new(forward, backward);
    }

    protected override void DropState()
    {
        base.DropState();
        forwardStorage.Clear();
        backwardStorage.Clear();
    }

    protected override void RelaxForwardVertex(IPathfindingVertex vertex)
    {
        forwardStorage.Add(vertex);
        SetForwardIntersection(vertex);
        base.RelaxForwardVertex(vertex);
    }

    protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
    {
        backwardStorage.Add(vertex);
        SetBackwardIntersections(vertex);
        base.RelaxBackwardVertex(vertex);
    }
}
