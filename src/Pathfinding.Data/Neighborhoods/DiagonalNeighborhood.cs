using Pathfinding.Data.Extensions;
using Pathfinding.Shared.Primitives;
using System.Diagnostics;

namespace Pathfinding.Data.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class DiagonalNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    protected override Coordinate[] Filter(Coordinate coordinate)
    {
        return !SelfCoordinate.IsCardinal(coordinate) ? [coordinate] : [];
    }
}