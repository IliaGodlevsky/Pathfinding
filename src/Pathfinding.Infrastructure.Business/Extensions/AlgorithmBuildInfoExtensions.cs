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
            Algorithm.AStar => info.ToAStartAlgorithm(range),
            Algorithm.AStarGreedy => info.ToAStarGreedyAlgorithm(range),
            Algorithm.AStarLee => info.ToAStarLeeAlgorithm(range),
            Algorithm.BidirectAStar => info.ToBidirectAStarAlgorithm(range),
            Algorithm.BidirectDijkstra => info.ToBidirectDijkstraAlgorithm(range),
            Algorithm.CostGreedy => info.ToCostGreedyAlgorithm(range),
            Algorithm.Dijkstra => info.ToDijkstraAlgorithm(range),
            Algorithm.DistanceFirst => info.ToDistanceFirstAlgorithm(range),
            Algorithm.Random => new RandomAlgorithm(range),
            Algorithm.BidirectRandom => new BidirectRandomAlgorithm(range),
            Algorithm.BidirectLee => new BidirectLeeAlgorithm(range),
            Algorithm.DepthFirst => new DepthFirstAlgorithm(range),
            Algorithm.DepthFirstRandom => new DepthRandomAlgorithm(range),
            Algorithm.Lee => new LeeAlgorithm(range),
            Algorithm.Snake => new SnakeAlgorithm(range),
            _ => throw new NotImplementedException($"Unknown algorithm: {info.Algorithm}")
        };
    }

    private static DistanceFirstAlgorithm ToDistanceFirstAlgorithm(this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new(range, info.GetHeuristic());
    }

    private static CostGreedyAlgorithm ToCostGreedyAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new(range, info.GetStepRule());
    }

    private static BidirectDijkstraAlgorithm ToBidirectDijkstraAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new(range, info.GetStepRule());
    }

    private static DijkstraAlgorithm ToDijkstraAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new(range, info.GetStepRule());
    }
    
    private static AStarAlgorithm ToAStartAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new (range, info.GetStepRule(), info.GetHeuristic());
    }

    private static BidirectAStarAlgorithm ToBidirectAStarAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new(range, info.GetStepRule(), info.GetHeuristic());
    }

    private static AStarLeeAlgorithm ToAStarLeeAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new (range, info.GetHeuristic());
    }

    private static AStarGreedyAlgorithm ToAStarGreedyAlgorithm(
        this IAlgorithmBuildInfo info,
        IReadOnlyCollection<IPathfindingVertex> range)
    {
        return new(range, info.GetHeuristic(), info.GetStepRule());
    }

    private static IStepRule GetStepRule(this IAlgorithmBuildInfo info)
    {
        return info.StepRule switch
        {
            StepRules.Default => new DefaultStepRule(),
            StepRules.Landscape => new LandscapeStepRule(),
            _ => throw new NotImplementedException($"Unknown step rule: {info.StepRule}")
        };
    }

    private static IHeuristic GetHeuristic(this IAlgorithmBuildInfo info)
    {
        return info.Heuristics switch
        {
            Heuristics.Euclidean => new EuclideanDistance().WithWeight(info.Weight),
            Heuristics.Chebyshev => new ChebyshevDistance().WithWeight(info.Weight),
            Heuristics.Diagonal => new DiagonalDistance().WithWeight(info.Weight),
            Heuristics.Manhattan => new ManhattanDistance().WithWeight(info.Weight),
            _ => throw new NotImplementedException($"Unknown heuristic: {info.Heuristics}")
        };
    }
}
