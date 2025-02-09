using Pathfinding.Shared.Primitives;

namespace Pathfinding.App.Console.Messages.ViewModel
{
    internal sealed class QueryPathfindingRangeMessage
    {
        public IReadOnlyCollection<Coordinate> PathfindingRange { get; set; } = [];
    }
}
