namespace Pathfinding.Domain.Entities;

public class Vertex : IEntity<long>
{
    public long Id { get; set; }

    public int GraphId { get; set; }

    /// <summary>
    /// A csv string with comma seaprator
    /// </summary>
    public string Coordinates { get; set; }

    public int Cost { get; set; }

    public bool IsObstacle { get; set; }
}
