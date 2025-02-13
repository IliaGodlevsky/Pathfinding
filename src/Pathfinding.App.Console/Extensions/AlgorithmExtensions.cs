using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions
{
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
                _ => string.Empty
            };
        }

        public static int GetOrder(this Algorithms algorithm)
        {
            return algorithm switch
            {
                Algorithms.Dijkstra => 1,
                Algorithms.AStar => 2,
                Algorithms.BidirectDijkstra => 3,
                Algorithms.BidirectAStar => 4,
                Algorithms.Lee => 5,
                Algorithms.BidirectLee => 6,
                Algorithms.AStarLee => 7,
                Algorithms.DistanceFirst => 8,
                Algorithms.CostGreedy => 9,
                Algorithms.AStarGreedy => 10,
                Algorithms.DepthFirst => 11,
                Algorithms.Snake => 12,
                _ => int.MaxValue
            };
        }
    }
}
