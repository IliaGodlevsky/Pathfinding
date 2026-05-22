using Pathfinding.Service.Algorithms.GraphPaths;
using Pathfinding.Service.Algorithms.Heuristics;
using Pathfinding.Service.Algorithms.StepRules;
using Pathfinding.Service.Interface;
using System.Collections.Frozen;

namespace Pathfinding.Service.Algorithms;

public sealed class AStarGreedyAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic, IStepRule stepRule) : GreedyAlgorithm(pathfindingRange)
{
    public AStarGreedyAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new ChebyshevDistance(), new DefaultStepRule())
    {

    }

    protected override GraphPath GetSubPath()
    {
        return new(
            Traces.ToFrozenDictionary(),
            CurrentRange.Target,
            stepRule);
    }

    protected override double CalculateGreed(IPathfindingVertex vertex)
    {
        var heuristicResult = heuristic.Calculate(vertex, CurrentRange.Target);
        var stepCost = stepRule.CalculateStepCost(vertex, CurrentVertex);
        return heuristicResult + stepCost;
    }
}