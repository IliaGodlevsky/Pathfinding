using Pathfinding.Data;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Algorithms;

public sealed class DepthFirstAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : DepthAlgorithm(pathfindingRange)
{
    protected override IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors)
    {
        return neighbors.FirstOrDefault() ?? NullPathfindingVertex.Interface;
    }
}