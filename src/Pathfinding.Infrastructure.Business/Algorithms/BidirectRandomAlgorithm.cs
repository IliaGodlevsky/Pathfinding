using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class BidirectRandomAlgorithm(IReadOnlyCollection<IPathfindingVertex> range)
    : BidirectBreadthFirstAlgorithm<List<IPathfindingVertex>>(range)
{
    private readonly Random random = new(range.Count ^ range.Sum(x => x.Cost.CurrentCost));

    protected override void MoveNextVertex()
    {
        var forward = NullPathfindingVertex.Interface;
        var backward = NullPathfindingVertex.Interface;

        if (ForwardStorage.Count > 0)
        {
            var index = random.Next(ForwardStorage.Count);
            forward = ForwardStorage[index];
            ForwardStorage.RemoveAt(index);
        }
        if (BackwardStorage.Count > 0)
        {
            var index = random.Next(BackwardStorage.Count);
            backward = BackwardStorage[index];
            BackwardStorage.RemoveAt(index);
        }
        if (Equals(backward, NullPathfindingVertex.Interface)
            || Equals(forward, NullPathfindingVertex.Interface))
        {
            throw new DeadendVertexException();
        }
        Current = new(forward, backward);
    }

    protected override void DropState()
    {
        base.DropState();
        ForwardStorage.Clear();
        BackwardStorage.Clear();
    }

    protected override void RelaxForwardVertex(IPathfindingVertex vertex)
    {
        ForwardStorage.Add(vertex);
        SetForwardIntersection(vertex);
        base.RelaxForwardVertex(vertex);
    }

    protected override void RelaxBackwardVertex(IPathfindingVertex vertex)
    {
        BackwardStorage.Add(vertex);
        SetBackwardIntersections(vertex);
        base.RelaxBackwardVertex(vertex);
    }
}
