using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Infrastructure.Data.Pathfinding;

public class Graph<TVertex> : IGraph<TVertex>
    where TVertex : IVertex
{
    public static readonly Graph<TVertex> Empty = new();

    private readonly IReadOnlyDictionary<Coordinate, TVertex> vertices;

    public IReadOnlyList<int> DimensionsSizes { get; }

    public int Count { get; }

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

    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var vertex in vertices.Values)
        {
            var vert = graph.Get(vertex.Position);
            vert.IsObstacle = vertex.IsObstacle;
            vert.Cost = vertex.Cost.DeepClone();
            vert.Neighbors = [.. vertex.Neighbors
                .GetCoordinates()
                .Select(graph.Get)];
        }
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