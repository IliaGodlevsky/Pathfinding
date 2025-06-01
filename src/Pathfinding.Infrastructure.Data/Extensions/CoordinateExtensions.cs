using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.Extensions;

public static class CoordinateExtensions
{
    public static int GetX(this Coordinate coordinate)
    {
        return coordinate.ElementAtOrDefault(0);
    }

    public static int GetY(this Coordinate coordinate)
    {
        return coordinate.ElementAtOrDefault(1);
    }

    public static bool IsCardinal(this Coordinate self, Coordinate coordinate)
    {
        // Cardinal coordinate differs from the
        // central one only for single coordinate value
        var difference = self
            .Zip(coordinate, (x, y) => Math.Abs(x - y))
            .Sum();
        return difference == 1;
    }
}