namespace Pathfinding.Domain.Core.Entities;

public class Vertex : IEntity<long>
{
    public long Id { get; set; }

    public int GraphId { get; set; }

    public string Coordinates { get; set; }

    public int Cost { get; set; }

    public int UpperValueRange { get; set; }

    public int LowerValueRange { get; set; }

    public bool IsObstacle { get; set; }
}
