using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Data;

public class Graph<TVertex> : IGraph<TVertex>
    where TVertex : IVertex
{
    public static readonly Graph<TVertex> Empty = new();

    private readonly IReadOnlyDictionary<Coordinate, TVertex> vertices;

    public IReadOnlyList<int> DimensionsSizes { get; }

    public int Count { get; }

    public InclusiveValueRange<int> CostRange { get; set; }

    public Graph(int requiredNumberOfDimensions,
        IReadOnlyCollection<TVertex> vertices,
        IReadOnlyList<int> dimensionSizes)
    {
        DimensionsSizes = [.. dimensionSizes.TakeOrDefault(requiredNumberOfDimensions, 1)];
        Count = DimensionsSizes.AggregateOrDefault((x, y) => x * y);
        this.vertices = vertices.Take(Count).ToDictionary(vertex => vertex.Position);
    }

    public Graph(IReadOnlyCollection<TVertex> vertices,
        IReadOnlyList<int> dimensionSizes)
        : this(dimensionSizes.Count, vertices, dimensionSizes)
    {

    }

    public Graph(IReadOnlyCollection<TVertex> vertices,
        params int[] dimensionSizes)
        : this(vertices, (IReadOnlyList<int>)dimensionSizes)
    {

    }

    protected Graph() : this([])
    {

    }

    public TVertex Get(Coordinate coordinate)
    {
        return vertices.TryGetValue(coordinate, out var vertex)
            ? vertex
            : throw new KeyNotFoundException();
    }

    public IEnumerator<TVertex> GetEnumerator()
    {
        return vertices.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}