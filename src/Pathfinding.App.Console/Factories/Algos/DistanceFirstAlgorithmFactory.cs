using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class DistanceFirstAlgorithmFactory(
    IHeuristicsFactory heuristicsFactory) 
    : IAlgorithmFactory<DistanceFirstAlgorithm>
{
    public DistanceFirstAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range, 
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));
        ArgumentNullException.ThrowIfNull(info.Weight, nameof(info.Weight));

        var heuristics = heuristicsFactory.CreateHeuristic(info.Heuristics.Value, info.Weight.Value);
        return new(range, heuristics);
    }
}