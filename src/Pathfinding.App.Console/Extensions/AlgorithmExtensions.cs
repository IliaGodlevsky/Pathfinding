using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions
{
    internal static class AlgorithmExtensions
    {
        private static Dictionary<Algorithms, int> OrderMap { get; }

        static AlgorithmExtensions()
        {
            Algorithms[] orders = 
            [
                Algorithms.Dijkstra,
                Algorithms.AStar,
                Algorithms.BidirectDijkstra,
                Algorithms.BidirectAStar,
                Algorithms.Random,
                Algorithms.BidirectRandom,
                Algorithms.Lee,
                Algorithms.BidirectLee,
                Algorithms.AStarLee,
                Algorithms.DistanceFirst,
                Algorithms.CostGreedy,
                Algorithms.AStarGreedy,
                Algorithms.DepthFirst,
                Algorithms.Snake
            ];

            OrderMap = orders
                .Select((Algorithm, Index) => (Algorithm, Index))
                .ToDictionary(x => x.Algorithm, x => x.Index);
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
                Algorithms.BidirectRandom => "Bi Random",
                _ => string.Empty
            };
        }

        public static int GetOrder(this Algorithms algorithm)
        {
            return OrderMap.GetValueOrDefault(algorithm, int.MaxValue);
        }
    }
}
