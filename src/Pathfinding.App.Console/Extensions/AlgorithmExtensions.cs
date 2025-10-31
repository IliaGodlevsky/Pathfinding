using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions;

internal static class AlgorithmExtensions
{
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
            Algorithms.IdaStar => "IDA*",
            Algorithms.BeamSearch => Resource.BeamSearch,
            _ => string.Empty
        };
    }
}
