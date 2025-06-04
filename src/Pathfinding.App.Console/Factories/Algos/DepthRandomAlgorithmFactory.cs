using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

internal sealed class DepthRandomAlgorithmFactory 
    : IAlgorithmFactory<DepthRandomAlgorithm>
{
    public DepthRandomAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range, 
        IAlgorithmBuildInfo info)
    {
        return new(range);
    }
}