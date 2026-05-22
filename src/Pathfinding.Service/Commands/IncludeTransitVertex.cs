using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Commands;

public sealed class IncludeTransitVertex<TVertex> : IPathfindingRangeCommand<TVertex>
    where TVertex : IVertex
{
    public void Execute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        range.Transit.Add(vertex);
    }

    public bool CanExecute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        return range.HasSourceAndTargetSet()
            && range.CanBeInRange(vertex);
    }
}