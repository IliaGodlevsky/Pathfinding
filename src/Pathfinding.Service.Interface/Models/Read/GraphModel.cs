using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface.Models.Read;

public record class GraphModel<T> : IGraphLayersInfo
    where T : IVertex
{
    public static readonly GraphModel<T> Empty = new()
    {
        Id = 0,
        Name = string.Empty,
        Vertices = [],
        DimensionSizes = []
    };

    public int Id { get; set; }

    public string Name { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public GraphStatuses Status { get; set; }

    public IReadOnlyCollection<T> Vertices { get; set; }

    public IReadOnlyList<int> DimensionSizes { get; set; }
}