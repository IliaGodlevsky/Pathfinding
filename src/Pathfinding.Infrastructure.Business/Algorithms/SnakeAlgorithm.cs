﻿using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Algorithms;

public sealed class SnakeAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange,
    IHeuristic heuristic) : GreedyAlgorithm(pathfindingRange)
{
    public SnakeAlgorithm(IReadOnlyCollection<IPathfindingVertex> pathfindingRange)
        : this(pathfindingRange, new ManhattanDistance())
    {

    }

    protected override double CalculateGreed(IPathfindingVertex vertex)
    {
        return heuristic.Calculate(vertex, CurrentRange.Source);
    }
}
