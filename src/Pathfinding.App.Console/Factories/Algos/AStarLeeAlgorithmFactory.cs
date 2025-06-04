using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class AStarLeeAlgorithmFactory(
    IHeuristicsFactory heuristicsFactory) 
    : IAlgorithmFactory<AStarLeeAlgorithm>
{
    public AStarLeeAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));
        ArgumentNullException.ThrowIfNull(info.Weight, nameof(info.Weight));

        var heuristics = heuristicsFactory.CreateHeuristic(info.Heuristics.Value, info.Weight.Value);
        return new(range, heuristics);
    }
}