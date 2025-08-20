using Pathfinding.App.Console.Extensions;
using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class IdaStarAlgorithmFactory(
    IStepRuleFactory stepRuleFactory,
    IHeuristicsFactory heuristicFactory) 
    : IAlgorithmFactory<IdaStarAlgorithm>
{
    public IdaStarAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));
        ArgumentNullException.ThrowIfNull(info.Weight, nameof(info.Weight));
        ArgumentNullException.ThrowIfNull(info.StepRule, nameof(info.StepRule));

        var heuristics = heuristicFactory.CreateHeuristic(info);
        var stepRule = stepRuleFactory.CreateStepRule(info.StepRule.Value);
        return new(range, stepRule, heuristics);
    }
}
