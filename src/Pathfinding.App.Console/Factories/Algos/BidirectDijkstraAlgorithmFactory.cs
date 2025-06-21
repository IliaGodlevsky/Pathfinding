using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class BidirectDijkstraAlgorithmFactory(IStepRuleFactory stepRuleFactory) : IAlgorithmFactory<BidirectDijkstraAlgorithm>
{
    public BidirectDijkstraAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range, 
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.StepRule, nameof(info.StepRule));

        var stepRule = stepRuleFactory.CreateStepRule(info.StepRule.Value);
        return new(range, stepRule);
    }
}