using Pathfinding.Data;
using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface;
using Pathfinding.Serialization.Models;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.Serialization.Extensions;

public static class MappingExtensions
{
    public static VertexSerializationModel ToSerializationModel<T>(this T vertex)
        where T : IVertex
    {
        var cost = vertex.Cost;
        return new()
        {
            IsObstacle = vertex.IsObstacle,
            Cost = new() { Cost = cost.CurrentCost },
            Position = new() { Coordinate = vertex.Position }
        };
    }

    public static GraphSerializationModel ToSerializationModel<T>(this GraphModel<T> model)
        where T : IVertex
    {
        return new()
        {
            DimensionSizes = model.DimensionSizes,
            Neighborhood = model.Neighborhood,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            Name = model.Name,
            CostRange = model.CostRange
        };
    }

    public static IReadOnlyCollection<VertexSerializationModel> ToSerializationModels<T>(this IEnumerable<T> vertices)
        where T : IVertex
    {
        return vertices.Select(x => x.ToSerializationModel()).ToList().AsReadOnly();
    }

    public static T ToVertex<T>(this VertexSerializationModel model)
        where T : IVertex, new()
    {
        var cost = model.Cost;
        return new()
        {
            Cost = new VertexCost(cost.Cost),
            IsObstacle = model.IsObstacle,
            Position = new(model.Position.Coordinate)
        };
    }

    public static IReadOnlyCollection<T> ToVertices<T>(this IEnumerable<VertexSerializationModel> vertices)
        where T : IVertex, new()
    {
        return vertices.Select(x => x.ToVertex<T>()).ToList().AsReadOnly();
    }

    public static RunStatisticsSerializationModel ToSerializationModel(this Statistics model)
    {
        return new()
        {
            Algorithm = model.Algorithm,
            Cost = model.Cost,
            Elapsed = TimeSpan.FromMilliseconds(model.Elapsed),
            Heuristics = model.Heuristics,
            ResultStatus = model.ResultStatus,
            StepRule = model.StepRule,
            Steps = model.Steps,
            Visited = model.Visited,
            Weight = model.Weight
        };
    }

    public static Statistics ToStatistics(this RunStatisticsSerializationModel model)
    {
        return new()
        {
            Algorithm = model.Algorithm,
            Cost = model.Cost,
            Visited = model.Visited,
            Heuristics = model.Heuristics,
            StepRule = model.StepRule,
            Steps = model.Steps,
            ResultStatus = model.ResultStatus,
            Elapsed = model.Elapsed.TotalMilliseconds,
            Weight = model.Weight
        };
    }

    public static IReadOnlyCollection<Statistics> ToStatistics(this IEnumerable<RunStatisticsSerializationModel> models)
    {
        return models.Select(x => x.ToStatistics()).ToList().AsReadOnly();
    }
}
