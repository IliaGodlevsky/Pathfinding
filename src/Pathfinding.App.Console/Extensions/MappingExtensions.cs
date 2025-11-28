using Pathfinding.App.Console.Models;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Undefined;

namespace Pathfinding.App.Console.Extensions;

internal static class MappingExtensions
{
    public static GraphInfoModel ToGraphInfo(this GraphInformationModel model)
    {
        return new()
        {
            Id = model.Id,
            Name = model.Name,
            Neighborhood = model.Neighborhood,
            SmoothLevel = model.SmoothLevel,
            Width = model.Dimensions.ElementAtOrDefault(0),
            Length = model.Dimensions.ElementAtOrDefault(1),
            ObstaclesCount = model.ObstaclesCount,
            Status = model.Status
        };
    }

    public static GraphInformationModel ToGraphInformationModel<T>(this GraphModel<T> model)
        where T : IVertex
    {
        return new()
        {
            Name = model.Name,
            Neighborhood = model.Neighborhood,
            Dimensions = model.DimensionSizes,
            Id = model.Id,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            ObstaclesCount = model.Vertices.Count(x => x.IsObstacle)
        };
    }

    public static GraphInfoModel[] ToGraphInfo(this IEnumerable<GraphModel<GraphVertexModel>> models)
    {
        return models.Select(x => x.ToGraphInformationModel()).ToGraphInfo();
    }

    public static GraphInfoModel[] ToGraphInfo(this IEnumerable<GraphInformationModel> models)
    {
        return [.. models.Select(ToGraphInfo)];
    }

    public static RunInfoModel ToRunInfo(this RunStatisticsModel model)
    {
        return new()
        {
            Id = model.Id,
            Algorithm = model.Algorithm,
            Cost = model.Cost,
            Steps = model.Steps,
            ResultStatus = model.ResultStatus,
            StepRule = model.StepRule,
            Heuristics = model.Heuristics,
            Weight = model.Weight,
            Elapsed = model.Elapsed,
            Visited = model.Visited
        };
    }

    public static IReadOnlyCollection<RunInfoModel> ToRunInfo(this IEnumerable<RunStatisticsModel> models)
    {
        return [.. models.Select(ToRunInfo)];
    }
}
