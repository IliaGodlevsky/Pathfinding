using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Commands;

public class ExcludeTargetVertex<TVertex> : IPathfindingRangeCommand<TVertex>
    where TVertex : IVertex
{
    public void Execute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        range.Target = default;
    }

    public bool CanExecute(IPathfindingRange<TVertex> range, TVertex vertex)
    {
        return range.Target?.Equals(vertex) == true;
    }
}