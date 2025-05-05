using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Service.Interface.Models.Read;

public record GraphInformationModel : IGraphLayersInfo
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public GraphStatuses Status { get; set; }

    public IReadOnlyList<int> Dimensions { get; set; }

    public int ObstaclesCount { get; set; }
}