using Pathfinding.Shared.Primitives;
using System.Diagnostics;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class HexagonalNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    private IReadOnlyCollection<Coordinate>? neighbors;

    protected override IReadOnlyCollection<Coordinate> Filter(Coordinate coordinate)
    {
        return neighbors ??= CreateNeighbors();
    }

    private IReadOnlyCollection<Coordinate> CreateNeighbors()
    {
        if (SelfCoordinate.Count < 2)
        {
            return Array.Empty<Coordinate>();
        }

        var dimensions = SelfCoordinate.Count;
        var result = new Coordinate[6];
        result[0] = Offset(1, 0, dimensions);
        result[1] = Offset(-1, 0, dimensions);
        result[2] = Offset(0, 1, dimensions);
        result[3] = Offset(0, -1, dimensions);
        result[4] = Offset(1, -1, dimensions);
        result[5] = Offset(-1, 1, dimensions);
        return result;
    }

    private Coordinate Offset(int dx, int dy, int dimensions)
    {
        var values = new int[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            values[i] = SelfCoordinate[i];
        }

        values[0] += dx;
        values[1] += dy;
        return new(values);
    }
}
