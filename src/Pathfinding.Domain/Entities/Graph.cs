using Pathfinding.Domain.Enums;

namespace Pathfinding.Domain.Entities;

public class Graph : IEntity<int>
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public GraphStatuses Status { get; set; }

    public int UpperValueRange { get; set; }

    public int LowerValueRange { get; set; }

    /// <summary>
    /// A csv string with comma separator
    /// </summary>
    public string Dimensions { get; set; }
}
