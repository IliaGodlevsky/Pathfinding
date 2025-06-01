using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Data.Extensions;

public static class PathfindingRangeExtensions
{
    public static bool CanBeInRange<TVertex>(this IPathfindingRange<TVertex> range, TVertex vertex)
        where TVertex : IVertex
    {
        return !vertex.IsIsolated() && !range.Contains(vertex);
    }

    public static bool HasSourceAndTargetSet<TVertex>(this IPathfindingRange<TVertex> range)
        where TVertex : IVertex
    {
        return range.Source?.IsIsolated() == false && range.Target?.IsIsolated() == false;
    }
}