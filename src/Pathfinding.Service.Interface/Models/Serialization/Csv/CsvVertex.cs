namespace Pathfinding.Service.Interface.Models.Serialization.Csv;

internal class CsvVertex
{
    public int GraphId { get; set; }

    public string Coordinate { get; set; }

    public int Cost { get; set; }

    public int UpperValueOfRange { get; set; }

    public int LowerValueOfRange { get; set; }

    public bool IsObstacle { get; set; }
}