using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Commands;

public sealed class ReplaceIsolatedSourceVertex<TVertex> : IPathfindingRangeCommand<TVertex>
    where TVertex : IVertex
{
    public void Execute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        range.Source = default;
        range.Source = vertex;
    }

    public bool CanExecute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        return range.Source != null
            && range.Source.IsIsolated()
            && range.CanBeInRange(vertex);
    }
}