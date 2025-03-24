namespace Pathfinding.Domain.Interface;

public interface IPathfindingRange<TVertex> : IEnumerable<TVertex>
    where TVertex : IVertex
{
    TVertex Source { get; set; }

    TVertex Target { get; set; }

    IList<TVertex> Transit { get; }
}
