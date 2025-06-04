using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Factories.Algos;

public sealed class SnakeAlgorithmFactory 
    : IAlgorithmFactory<SnakeAlgorithm>
{
    public SnakeAlgorithm CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range, 
        IAlgorithmBuildInfo info)
    {
        return new(range);
    }
}