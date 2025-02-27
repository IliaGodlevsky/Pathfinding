using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Interface;

public interface IGraphPath : IReadOnlyCollection<Coordinate>
{
    double Cost { get; }
}
