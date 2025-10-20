using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class BidirectRandomAlgorithmFactory
    : IAlgorithmFactory<BidirectRandomAlgorithm>
{
    public BidirectRandomAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        return new(range);
    }
}