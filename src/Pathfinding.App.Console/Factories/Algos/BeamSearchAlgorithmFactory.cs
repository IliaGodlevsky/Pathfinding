using System;
using System.Collections.Generic;
using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class BeamSearchAlgorithmFactory(IHeuristicsFactory heuristicsFactory)
    : IAlgorithmFactory<BeamSearchAlgorithm>
{
    public BeamSearchAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        ArgumentNullException.ThrowIfNull(info.Heuristics, nameof(info.Heuristics));

        var weight = info.Weight ?? 1;
        var heuristic = heuristicsFactory.CreateHeuristic(info.Heuristics.Value, weight);
        return new(range, heuristic);
    }
}
