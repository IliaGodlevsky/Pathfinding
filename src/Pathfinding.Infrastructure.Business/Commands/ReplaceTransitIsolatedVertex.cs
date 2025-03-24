using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Extensions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Commands;

public sealed class ReplaceTransitIsolatedVertex<TVertex> : IPathfindingRangeCommand<TVertex>
    where TVertex : IVertex
{
    public void Execute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        var isolated = range.Transit.First(IsIsolated);
        int isolatedIndex = range.Transit.IndexOf(isolated);
        range.Transit.Remove(isolated);
        range.Transit.Insert(isolatedIndex, vertex);
    }

    public bool CanExecute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        return range.Transit.Count > 0
            && range.HasSourceAndTargetSet()
            && range.Transit.Any(IsIsolated)
            && range.CanBeInRange(vertex);
    }

    private static bool IsIsolated(TVertex vertex)
    {
        return vertex.IsIsolated();
    }
}