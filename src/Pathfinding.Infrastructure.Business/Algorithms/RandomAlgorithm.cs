using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms
{
    public sealed class RandomAlgorithm(IEnumerable<IPathfindingVertex> range)
        : BreadthFirstAlgorithm<List<IPathfindingVertex>>(range)
    {
        private readonly Random random = new(range.Count() 
            ^ range.Aggregate(0, (x, y) => x + y.Cost.CurrentCost));

        protected override void MoveNextVertex()
        {
            if (storage.Count > 0)
            {
                int index = random.Next(storage.Count);
                CurrentVertex = storage[index];
                storage.RemoveAt(index);
                return;
            }
            throw new DeadendVertexException();
        }

        protected override void DropState()
        {
            base.DropState();
            storage.Clear();
        }

        protected override void RelaxVertex(IPathfindingVertex vertex)
        {
            storage.Add(vertex);
            base.RelaxVertex(vertex);
        }
    }
}
