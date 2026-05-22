using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Presentation.Console.Factories;

public interface IAlgorithmFactory<out T>
    where T : IPathfindingAlgorithm<IEnumerable<Coordinate>>
{
    T Create(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info);
}