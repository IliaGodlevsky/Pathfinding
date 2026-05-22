using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Commands;

public sealed class IncludeSourceVertex<TVertex> : IPathfindingRangeCommand<TVertex>
    where TVertex : IVertex
{
    public void Execute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        range.Source = vertex;
    }

    public bool CanExecute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        return range.Source == null && range.CanBeInRange(vertex);
    }
}