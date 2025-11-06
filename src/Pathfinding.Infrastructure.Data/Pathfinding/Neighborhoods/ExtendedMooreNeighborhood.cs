using Pathfinding.Shared.Primitives;
using System.Diagnostics;
using System.Linq;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class ExtendedMooreNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    private IReadOnlyCollection<Coordinate>? neighbors;

    protected override IReadOnlyCollection<Coordinate> Filter(Coordinate coordinate)
    {
        return neighbors ??= CreateNeighbors();
    }

    private IReadOnlyCollection<Coordinate> CreateNeighbors()
    {
        var dimension = SelfCoordinate.Count;
        var deltas = new int[dimension];
        var coordinates = new HashSet<Coordinate>();

        void Collect(int depth)
        {
            if (depth == dimension)
            {
                if (deltas.All(offset => offset == 0))
                {
                    return;
                }

                if (deltas.Select(Math.Abs).Max() <= 2)
                {
                    var values = new int[dimension];
                    for (int i = 0; i < dimension; i++)
                    {
                        values[i] = SelfCoordinate[i] + deltas[i];
                    }

                    coordinates.Add(new(values));
                }

                return;
            }

            for (int offset = -2; offset <= 2; offset++)
            {
                deltas[depth] = offset;
                Collect(depth + 1);
            }
        }

        Collect(0);
        return coordinates;
    }
}
