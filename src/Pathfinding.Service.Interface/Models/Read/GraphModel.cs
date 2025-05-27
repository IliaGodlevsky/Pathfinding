using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Service.Interface.Models.Read;

public record GraphModel<T>
    where T : IVertex
{
    public int Id { get; set; }

    public string Name { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public GraphStatuses Status { get; set; }

    public IReadOnlyCollection<T> Vertices { get; set; }

    public IReadOnlyList<int> DimensionSizes { get; set; }
}