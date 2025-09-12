using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;

public abstract class Neighborhood : INeighborhood
{
    protected readonly Coordinate SelfCoordinate;
    private readonly int limitDepth;
    private readonly int[] resultCoordinatesValues;
    private readonly Lazy<HashSet<Coordinate>> neighbourhood;

    public int Count => neighbourhood.Value.Count;

    protected Neighborhood(Coordinate coordinate)
    {
        SelfCoordinate = coordinate;
        limitDepth = SelfCoordinate.Count;
        resultCoordinatesValues = new int[limitDepth];
        neighbourhood = new(GetNeighborhood, true);
    }

    private HashSet<Coordinate> CollectNeighbors(int depth = 0)
    {
        var neighborhood = new HashSet<Coordinate>();
        for (int offset = -1; offset <= 1; offset++)
        {
            resultCoordinatesValues[depth] = SelfCoordinate[depth] + offset;
            var neighbours = IsBottom(depth)
                ? CollectNeighbors(depth + 1).AsEnumerable()
                : Filter(new(resultCoordinatesValues));
            neighborhood.AddRange(neighbours);
        }
        return neighborhood;
    }

    protected abstract IReadOnlyCollection<Coordinate> Filter(Coordinate coordinate);

    private bool IsBottom(int depth)
    {
        return depth < limitDepth - 1;
    }

    private HashSet<Coordinate> GetNeighborhood()
    {
        var coordinates = CollectNeighbors();
        coordinates.Remove(SelfCoordinate);
        return coordinates;
    }

    public IEnumerator<Coordinate> GetEnumerator()
    {
        return neighbourhood.Value.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}