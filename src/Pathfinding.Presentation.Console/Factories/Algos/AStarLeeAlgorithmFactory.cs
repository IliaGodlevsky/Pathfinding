using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Factories.Algos;

public sealed class AStarLeeAlgorithmFactory(
    IHeuristicsFactory heuristicsFactory)
    : IAlgorithmFactory<AStarLeeAlgorithm>
{
    public AStarLeeAlgorithm Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));

        var heuristics = heuristicsFactory.Create(info.Heuristics.Value, 1);
        return new(range, heuristics);
    }
}