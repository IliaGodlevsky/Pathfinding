using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class DistanceFirstAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic) : GreedyAlgorithm(pathfindingRange)
{
    public DistanceFirstAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new EuclidianDistance())
    {

    }

    protected override double CalculateGreed(IPathfindingVertex vertex)
    {
        return heuristic.Calculate(vertex, CurrentRange.Target);
    }
}