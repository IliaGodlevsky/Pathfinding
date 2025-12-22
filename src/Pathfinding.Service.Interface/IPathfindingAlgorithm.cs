using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Interface;

public interface IPathfindingAlgorithm<out TPath>
    where TPath : IEnumerable<Coordinate>
{
    TPath FindPath();
}