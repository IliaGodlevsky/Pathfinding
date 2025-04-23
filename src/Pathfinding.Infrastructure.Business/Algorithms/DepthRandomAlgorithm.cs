using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class DepthRandomAlgorithm(IReadOnlyCollection<IPathfindingVertex> range)
    : DepthAlgorithm(range)
{
    private readonly Random random = new(range.Count ^ range.Sum(y => y.Cost.CurrentCost));

    protected override IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors)
    {
        int index = random.Next(neighbors.Count);
        return neighbors.ElementAtOrDefault(index) ?? NullPathfindingVertex.Interface;
    }
}
