using System.Diagnostics;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class DiagonalNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    protected override Coordinate[] Filter(Coordinate coordinate)
    {
        return !SelfCoordinate.IsCardinal(coordinate) ? [coordinate] : [];
    }
}