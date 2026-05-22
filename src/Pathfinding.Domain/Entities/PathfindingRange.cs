namespace Pathfinding.Domain.Entities;

public class PathfindingRange : IEntity<int>
{
    public int Id { get; set; }

    public int GraphId { get; set; }

    public long VertexId { get; set; }

    public int Order { get; set; }
}
