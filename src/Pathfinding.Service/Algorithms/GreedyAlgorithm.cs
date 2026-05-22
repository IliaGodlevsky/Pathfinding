using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Algorithms;

public abstract class GreedyAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
    : DepthAlgorithm(pathfindingRange)
{
    protected abstract double CalculateGreed(IPathfindingVertex vertex);

    protected override IPathfindingVertex GetVertex(IReadOnlyCollection<IPathfindingVertex> neighbors)
    {
        double leastVertexCost = neighbors.Count > 0 ? neighbors.Min(CalculateGreed) : 0;
        return neighbors.FirstOrNullVertex(vertex => CalculateGreed(vertex) == leastVertexCost);
    }
}
