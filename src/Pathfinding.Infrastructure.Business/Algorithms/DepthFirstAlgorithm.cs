using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class DepthFirstAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
    : DepthAlgorithm(pathfindingRange)
{
    protected override IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors)
    {
        return neighbors.FirstOrDefault() ?? NullPathfindingVertex.Interface;
    }
}