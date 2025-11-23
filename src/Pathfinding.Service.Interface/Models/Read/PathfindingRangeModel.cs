namespace Pathfinding.Service.Interface.Models.Read;

public class PathfindingRangeModel
{
    public int Id { get; set; }

    public bool IsSource { get; set; }

    public bool IsTarget { get; set; }

    public long VertexId { get; set; }

    public int GraphId { get; set; }

    public int Order { get; set; }
}