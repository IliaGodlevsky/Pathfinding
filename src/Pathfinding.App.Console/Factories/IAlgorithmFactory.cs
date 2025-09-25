using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.App.Console.Factories;

public interface IAlgorithmFactory<out T> 
    where T : IAlgorithm<IEnumerable<Coordinate>>
{
    T CreateAlgorithm(
        IReadOnlyCollection<IPathfindingVertex> range,
        IAlgorithmBuildInfo info);
}