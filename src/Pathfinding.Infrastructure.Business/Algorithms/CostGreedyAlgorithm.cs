using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Service.Interface;
using System.Collections.Frozen;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class CostGreedyAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IStepRule stepRule) : GreedyAlgorithm(pathfindingRange)
{
    public CostGreedyAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new DefaultStepRule())
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
        return stepRule.CalculateStepCost(vertex, CurrentVertex);
    }
}