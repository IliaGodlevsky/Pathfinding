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
                    foreach (int sign1 in new[] { +2, -2 })
                    {
                        foreach (int sign2 in new[] { +1, -1 })
                        {
                            var delta = new int[coordinate.Count];
                            delta[i] = sign1;
                            delta[j] = sign2;

                            var neighbor = SelfCoordinate.Zip(delta, (x, d) => x + d);
                            neighbors.Add(new(neighbor));
                        }
                    }
                }
            }
        }

        return neighbors;
    }
}