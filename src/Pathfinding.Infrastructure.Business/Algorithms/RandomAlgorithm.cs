using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class RandomAlgorithm(IReadOnlyCollection<IPathfindingVertex> range)
    : BreadthFirstAlgorithm<List<IPathfindingVertex>>(range)
{
    private readonly Random random = new(range.Count ^ range.Sum(y => y.Cost.CurrentCost));

    protected override void MoveNextVertex()
    {
        if (Storage.Count > 0)
        {
            int index = random.Next(Storage.Count);
            CurrentVertex = Storage[index];
            Storage.RemoveAt(index);
            return;
        }
        throw new DeadendVertexException();
    }

    protected override void DropState()
    {
        base.DropState();
        Storage.Clear();
    }

    protected override void RelaxVertex(IPathfindingVertex vertex)
    {
        Storage.Add(vertex);
        base.RelaxVertex(vertex);
    }
}
