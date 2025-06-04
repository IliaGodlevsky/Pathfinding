using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class BidirectLeeAlgorithmFactory : IAlgorithmFactory<BidirectLeeAlgorithm>
{
    public BidirectLeeAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range, 
        IAlgorithmBuildInfo info)
    {
        return new(range);
    }
}