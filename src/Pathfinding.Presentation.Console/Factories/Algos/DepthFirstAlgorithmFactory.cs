using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Factories.Algos;

public sealed class DepthFirstAlgorithmFactory
    : IAlgorithmFactory<DepthFirstAlgorithm>
{
    public DepthFirstAlgorithm Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info)
    {
        return new(range);
    }
}