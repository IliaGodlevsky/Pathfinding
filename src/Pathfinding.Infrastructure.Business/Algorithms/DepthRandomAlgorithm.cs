using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms
{
    public sealed class DepthRandomAlgorithm(IEnumerable<IPathfindingVertex> range)
        : DepthAlgorithm(range)
    {
        private readonly Random random = new(range.Count()
            ^ range.Aggregate(0, (x, y) => x + y.Cost.CurrentCost));

        protected override IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors)
        {
            int index = random.Next(neighbors.Count);
            return neighbors.ElementAtOrDefault(index) ?? NullPathfindingVertex.Interface;
        }
    }
}
