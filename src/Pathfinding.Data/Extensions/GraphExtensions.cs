using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Data.Extensions;

public static class GraphExtensions
{
    public static TVertex Get<TVertex>(this IGraph<TVertex> graph, int x, int y)
        where TVertex : IVertex
    {
        return graph.Get(new(x, y));
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

    public static int GetDepth<TVertex>(this IGraph<TVertex> graph)
        where TVertex : IVertex
    {
        int depth = graph.DimensionsSizes.ElementAtOrDefault(2);
        if (graph.GetWidth() > 0 && graph.GetLength() > 0)
        {
            return depth > 0 ? depth : 1;
        }
        return depth;
    }
}