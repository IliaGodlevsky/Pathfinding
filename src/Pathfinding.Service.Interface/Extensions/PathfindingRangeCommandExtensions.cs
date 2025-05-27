using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface.Extensions;

public static class PathfindingRangeCommandExtensions
{
    public static void ExecuteFirst<T>(this IEnumerable<IPathfindingRangeCommand<T>> commands,
        IPathfindingRange<T> range, T vertex)
        where T : IVertex
    {
        commands
            .FirstOrDefault(x => x.CanExecute(range, vertex))
            ?.Execute(range, vertex);
    }
}