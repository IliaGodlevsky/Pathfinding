﻿using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions;

internal static class AlgorithmExtensions
{
    private static Dictionary<Algorithms, int> OrderMap { get; }

    static AlgorithmExtensions()
    {
        Algorithms[] orders =
        [
            // Wave group
            Algorithms.Dijkstra,
            Algorithms.AStar,
            Algorithms.BidirectDijkstra,
            Algorithms.BidirectAStar,
            Algorithms.CostGreedy,
            Algorithms.AStarGreedy,
            // Breadth group
            Algorithms.Lee,
            Algorithms.BidirectLee,
            Algorithms.AStarLee,
            Algorithms.DistanceFirst,
            Algorithms.DepthFirst,
            Algorithms.DepthFirstRandom,
            Algorithms.Snake,
            // Random group
            Algorithms.Random,
            Algorithms.BidirectRandom
        ];

        OrderMap = orders
            .Select((algorithm, index) => (algorithm, index))
            .ToDictionary(x => x.algorithm, x => x.index);
    }

    public static string ToStringRepresentation(this Algorithms algorithm)
    {
        return algorithm switch
        {
            Algorithms.Dijkstra => Resource.Dijkstra,
            Algorithms.BidirectDijkstra => Resource.BidirectDijkstra,
            Algorithms.AStar => Resource.AStar,
            Algorithms.BidirectAStar => Resource.BidirectAStar,
            Algorithms.Lee => Resource.Lee,
            Algorithms.BidirectLee => Resource.BidirectLee,
            Algorithms.AStarLee => Resource.AStarLee,
            Algorithms.DistanceFirst => Resource.DistanceFirst,
            Algorithms.CostGreedy => Resource.CostGreedy,
            Algorithms.AStarGreedy => Resource.AStarGreedy,
            Algorithms.DepthFirst => Resource.DepthFirst,
            Algorithms.Snake => Resource.Snake,
            Algorithms.Random => Resource.RandomAlgorithm,
            Algorithms.BidirectRandom => Resource.BidirectRandom,
            Algorithms.DepthFirstRandom => Resource.DepthRandom,
            _ => string.Empty
        };
    }

    public static int GetOrder(this Algorithms algorithm)
    {
        return OrderMap.GetValueOrDefault(algorithm, int.MaxValue);
    }
}
