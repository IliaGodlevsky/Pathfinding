using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Factories.Algos;

public sealed class DijkstraAlgorithmFactory(IStepRuleFactory stepRuleFactory)
    : IAlgorithmFactory<DijkstraAlgorithm>
{
    public DijkstraAlgorithm Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.StepRule, nameof(info.StepRule));

        var stepRule = stepRuleFactory.Create(info.StepRule.Value);
        return new(range, stepRule);
    }
}