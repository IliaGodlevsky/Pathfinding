using Pathfinding.Shared.Primitives;
using System.Diagnostics;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class MooreNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    protected override Coordinate[] Filter(Coordinate coordinate)
    {
        return [coordinate];
    }
}