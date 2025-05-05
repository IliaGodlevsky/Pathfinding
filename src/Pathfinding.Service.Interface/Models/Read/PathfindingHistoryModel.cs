using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Interface.Models.Read;

public record PathfindingHistoryModel<T>
    where T : IVertex
{
    public GraphModel<T> Graph { get; set; }

    public IReadOnlyCollection<RunStatisticsModel> Statistics { get; set; }

    public IReadOnlyCollection<Coordinate> Range { get; set; }
}