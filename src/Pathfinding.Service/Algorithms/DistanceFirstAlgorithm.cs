using Pathfinding.Service.Algorithms.Heuristics;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Algorithms;

public sealed class DistanceFirstAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic) : GreedyAlgorithm(pathfindingRange)
{
    public DistanceFirstAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new EuclideanDistance())
    {

    }

    protected override double CalculateGreed(IPathfindingVertex vertex)
    {
        return heuristic.Calculate(vertex, CurrentRange.Target);
    }
}