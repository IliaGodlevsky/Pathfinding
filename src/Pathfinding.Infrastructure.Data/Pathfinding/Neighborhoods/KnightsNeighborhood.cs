using Pathfinding.Shared.Primitives;
using System.Diagnostics;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class KnightNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    protected override HashSet<Coordinate> Filter(Coordinate coordinate)
    {
        var neighbors = new HashSet<Coordinate>();
        for (int i = 0; i < coordinate.Count; i++)
        {
            for (int j = 0; j < coordinate.Count; j++)
            {
                if (i != j)
                {
                    for (int s1 = -1; s1 <= 1; s1 += 2)
                    {
                        for (int s2 = -1; s2 <= 1; s2 += 2)
                        {
                            var delta = new int[coordinate.Count];
                            delta[i] = 2 * s1;
                            delta[j] = 1 * s2;

                            var coords = SelfCoordinate.Zip(delta, (x, d) => x + d);
                            neighbors.Add(new Coordinate(coords));
                        }
                    }
                }
            }
        }

        return neighbors;
    }
}