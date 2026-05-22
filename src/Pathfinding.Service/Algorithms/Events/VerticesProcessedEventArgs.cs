using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Algorithms.Events;

public class VerticesProcessedEventArgs(IPathfindingVertex current,
    IEnumerable<IPathfindingVertex> enqueued) : EventArgs
{
    public Coordinate Current { get; } = current.Position;

    public IReadOnlyList<Coordinate> Enqueued { get; }
        = [.. enqueued.Select(x => x.Position)];
}
