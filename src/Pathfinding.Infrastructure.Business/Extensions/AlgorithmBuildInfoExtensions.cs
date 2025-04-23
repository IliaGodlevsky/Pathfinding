using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;
using Algorithm = Pathfinding.Domain.Core.Enums.Algorithms;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class AlgorithmBuildInfoExtensions
{
    public static PathfindingProcess ToAlgorithm(this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return info.Algorithm switch
        {
            Algorithm.AStar => new AStarAlgorithm(range,
                GetStepRule(info.StepRule),
                GetHeuristic(info.Heuristics, info.Weight)),
            Algorithm.Random => new RandomAlgorithm(range),
            Algorithm.BidirectRandom => new BidirectRandomAlgorithm(range),
            Algorithm.AStarGreedy => new AStarGreedyAlgorithm(range,
                GetHeuristic(info.Heuristics, info.Weight),
                GetStepRule(info.StepRule)),
            Algorithm.AStarLee => new AStarLeeAlgorithm(range,
                GetHeuristic(info.Heuristics, info.Weight)),
            Algorithm.BidirectAStar => new BidirectAStarAlgorithm(range,
                GetStepRule(info.StepRule),
                GetHeuristic(info.Heuristics, info.Weight)),
            Algorithm.BidirectDijkstra => new BidirectDijkstraAlgorithm(range,
                GetStepRule(info.StepRule)),
            Algorithm.BidirectLee => new BidirectLeeAlgorithm(range),
            Algorithm.CostGreedy => new CostGreedyAlgorithm(range,
                GetStepRule(info.StepRule)),
            Algorithm.DepthFirst => new DepthFirstAlgorithm(range),
            Algorithm.DepthFirstRandom => new DepthRandomAlgorithm(range),
            Algorithm.Dijkstra => new DijkstraAlgorithm(range,
                GetStepRule(info.StepRule)),
            Algorithm.DistanceFirst => new DistanceFirstAlgorithm(range,
                GetHeuristic(info.Heuristics, info.Weight)),
            Algorithm.Lee => new LeeAlgorithm(range),
            Algorithm.Snake => new SnakeAlgorithm(range, new ManhattanDistance()),
            _ => throw new NotImplementedException($"Unknown algorithm: {info.Algorithm}")
        };
    }

    private static IStepRule GetStepRule(StepRules? stepRules)
    {
        return stepRules switch
        {
            StepRules.Default => new DefaultStepRule(),
            StepRules.Landscape => new LandscapeStepRule(),
            _ => throw new NotImplementedException($"Unknown step rule: {stepRules}")
        };
    }

    private static IHeuristic GetHeuristic(Heuristics? heuristic, double? weight)
    {
        return heuristic switch
        {
            Heuristics.Euclidean => new EuclideanDistance().WithWeight(weight),
            Heuristics.Chebyshev => new ChebyshevDistance().WithWeight(weight),
            Heuristics.Diagonal => new DiagonalDistance().WithWeight(weight),
            Heuristics.Manhattan => new ManhattanDistance().WithWeight(weight),
            _ => throw new NotImplementedException($"Unknown heuristic: {heuristic}")
        };
    }
}
