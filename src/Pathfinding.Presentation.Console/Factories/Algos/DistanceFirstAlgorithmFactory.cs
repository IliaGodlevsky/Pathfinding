using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Factories.Algos;

public sealed class DistanceFirstAlgorithmFactory(
    IHeuristicsFactory heuristicsFactory)
    : IAlgorithmFactory<DistanceFirstAlgorithm>
{
    public DistanceFirstAlgorithm Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));

        var heuristics = heuristicsFactory.Create(info.Heuristics.Value, 1);
        return new(range, heuristics);
    }
}