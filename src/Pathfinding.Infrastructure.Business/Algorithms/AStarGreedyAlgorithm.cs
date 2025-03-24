using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Service.Interface;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class AStarGreedyAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic, IStepRule stepRule) : GreedyAlgorithm(pathfindingRange)
{
    public AStarGreedyAlgorithm(IEnumerable<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new ChebyshevDistance(), new DefaultStepRule())
    {

    }

    protected override IGraphPath GetSubPath()
    {
        return new GraphPath(traces.ToFrozenDictionary(),
            CurrentRange.Target, stepRule);
    }

    protected override double CalculateGreed(IPathfindingVertex vertex)
    {
        double heuristicResult = heuristic.Calculate(vertex, CurrentRange.Target);
        double stepCost = stepRule.CalculateStepCost(vertex, CurrentVertex);
        return heuristicResult + stepCost;
    }
}