using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Commands;

public sealed class IncludeTargetVertex<TVertex> : IPathfindingRangeCommand<TVertex>
    where TVertex : IVertex
{
    public void Execute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        range.Target = vertex;
    }

    public bool CanExecute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        return range.Source != null
            && range.Target == null
            && range.CanBeInRange(vertex);
    }
}
