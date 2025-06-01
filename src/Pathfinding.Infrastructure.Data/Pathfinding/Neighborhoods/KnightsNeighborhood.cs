using Pathfinding.Shared.Primitives;
using System.Diagnostics;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

[DebuggerDisplay("Count = {Count}")]
public sealed class KnightNeighborhood(Coordinate coordinate) : Neighborhood(coordinate)
{
    protected override IReadOnlyCollection<Coordinate> Filter(Coordinate coordinate)
    {
        var neighbors = new HashSet<Coordinate>();
        for (var i = 0; i < coordinate.Count; i++)
        {
            for (var j = 0; j < coordinate.Count; j++)
            {
                if (i != j)
                {
                    foreach (var sign1 in new[] { +2, -2 })
                    {
                        foreach (var sign2 in new[] { +1, -1 })
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