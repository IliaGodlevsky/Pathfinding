using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.Extensions;

public static class GraphExtensions
{
    public static TVertex Get<TVertex>(this IGraph<TVertex> graph, int x, int y)
        where TVertex : IVertex
    {
        return graph.Get(new Coordinate(x, y));
    }

    public static int GetWidth<TVertex>(this IGraph<TVertex> graph)
        where TVertex : IVertex
    {
        return graph.DimensionsSizes.ElementAtOrDefault(0);
    }

    public static int GetLength<TVertex>(this IGraph<TVertex> graph)
        where TVertex : IVertex
    {
        return graph.DimensionsSizes.ElementAtOrDefault(1);
    }

    public static IEnumerable<TVertex> GetObstacles<TVertex>(this IEnumerable<TVertex> graph)
        where TVertex : IVertex
    {
        return graph.Where(vertex => vertex.IsObstacle);
    }

    public static IEnumerable<Coordinate> GetCoordinates<TVertex>(this IEnumerable<TVertex> graph)
        where TVertex : IVertex
    {
        return graph.Select(vertex => vertex.Position);
    }

    public static int GetObstaclesCount<TVertex>(this IEnumerable<TVertex> self)
        where TVertex : IVertex
    {
        return self.GetObstacles().Count();
    }
}