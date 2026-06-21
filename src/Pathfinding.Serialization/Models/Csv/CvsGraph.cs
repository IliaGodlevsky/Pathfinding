using Pathfinding.Domain.Enums;

namespace Pathfinding.Serialization.Models.Csv;

internal class CsvGraph
{
    public int Id { get; set; }

    public string Name { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public GraphStatuses Status { get; set; }

    public string DimensionSizes { get; set; }

    public int UpperValueOfRange { get; set; }

    public int LowerValueOfRange { get; set; }
}