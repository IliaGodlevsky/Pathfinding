using Pathfinding.Shared.Primitives;

namespace Pathfinding.Domain.Interface;

public interface IVertex : IEquatable<IVertex>
{
    bool IsObstacle { get; set; }

    IVertexCost Cost { get; set; }

    Coordinate Position { get; set; }

    IReadOnlyCollection<IVertex> Neighbors { get; set; }
}