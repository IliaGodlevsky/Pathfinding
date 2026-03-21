using Pathfinding.Domain.Core.Enums;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Interface.Models.Read;

public record GraphInformationModel
{
    public int Id { get; set; }

    public string Name { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public GraphStatuses Status { get; set; }

    public IReadOnlyList<int> Dimensions { get; set; }

    public int ObstaclesCount { get; set; }

    public InclusiveValueRange<int> CostRange { get; set; }
}