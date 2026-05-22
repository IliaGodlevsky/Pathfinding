using Pathfinding.Presentation.Console.Extensions;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Factories.Algos;

public sealed class BidirectAStarAlgorithmFactory(
    IStepRuleFactory stepRuleFactory,
    IHeuristicsFactory heuristicFactory)
    : IAlgorithmFactory<BidirectAStarAlgorithm>
{
    public BidirectAStarAlgorithm Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));
        ArgumentNullException.ThrowIfNull(info.Weight, nameof(info.Weight));
        ArgumentNullException.ThrowIfNull(info.StepRule, nameof(info.StepRule));

        var heuristics = heuristicFactory.CreateHeuristic(info);
        var stepRule = stepRuleFactory.Create(info.StepRule.Value);
        return new(range, stepRule, heuristics);
    }
}
