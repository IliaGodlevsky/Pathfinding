using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Factories.Algos;

public sealed class BidirectDijkstraAlgorithmFactory(IStepRuleFactory stepRuleFactory) : IAlgorithmFactory<BidirectDijkstraAlgorithm>
{
    public BidirectDijkstraAlgorithm Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.StepRule, nameof(info.StepRule));

        var stepRule = stepRuleFactory.Create(info.StepRule.Value);
        return new(range, stepRule);
    }
}