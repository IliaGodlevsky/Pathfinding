using Pathfinding.Data;
using Pathfinding.Domain;
using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;

namespace Pathfinding.Service.Extensions;

public static class MappingExtensions
{
    public static IReadOnlyCollection<Statistics> ToStatistics(this IEnumerable<RunStatisticsModel> models)
    {
        return models.Select(x => x.ToStatistics()).ToList().AsReadOnly();
    }

    public static Statistics ToStatistics(this RunStatisticsModel model)
    {
        return new()
        {
            GraphId = model.GraphId,
            Algorithm = model.Algorithm,
            Cost = model.Cost,
            Elapsed = model.Elapsed.TotalMilliseconds,
            Id = model.Id,
            Visited = model.Visited,
            Heuristics = model.Heuristics,
            StepRule = model.StepRule,
            Steps = model.Steps,
            ResultStatus = model.ResultStatus,
            Weight = model.Weight
        };
    }

    public static PathfindingRange ToPathfindingRange(this CreatePathfindingVertexRequest request)
    {
        return new()
        {
            GraphId = request.GraphId,
            VertexId = request.VertexId,
            Order = request.Index
        };
    }

    public static Statistics ToStatistics(this CreateStatisticsRequest request)
    {
        return new()
        {
            Algorithm = request.Algorithm,
            ResultStatus = request.ResultStatus,
            StepRule = request.StepRule,
            Steps = request.Steps,
            Heuristics = request.Heuristics,
            Cost = request.Cost,
            Elapsed = request.Elapsed.TotalMilliseconds,
            GraphId = request.GraphId,
            Visited = request.Visited,
            Weight = request.Weight
        };
    }

    public static T ToVertex<T>(this Vertex vertexEntity)
        where T : IVertex, IEntity<long>, new()
    {
        return new()
        {
            Id = vertexEntity.Id,
            IsObstacle = vertexEntity.IsObstacle,
            Position = new([.. vertexEntity.Coordinates.Split(",").Select(int.Parse)]),
            Cost = new VertexCost(vertexEntity.Cost),
        };
    }

    public static PathfindingRangeModel ToRangeModel(this PathfindingRange entity)
    {
        return new()
        {
            Order = entity.Order,
            GraphId = entity.GraphId,
            VertexId = entity.VertexId
        };
    }

    public static RunStatisticsModel ToRunStatisticsModel(this Statistics entity)
    {
        return new()
        {
            Id = entity.Id,
            GraphId = entity.GraphId,
            Heuristics = entity.Heuristics,
            Weight = entity.Weight,
            StepRule = entity.StepRule,
            Steps = entity.Steps,
            Elapsed = TimeSpan.FromMilliseconds(entity.Elapsed),
            Visited = entity.Visited,
            Algorithm = entity.Algorithm,
            Cost = entity.Cost,
            ResultStatus = entity.ResultStatus
        };
    }

    public static IReadOnlyCollection<RunStatisticsModel> ToRunStatisticsModels(this IEnumerable<Statistics> entities)
    {
        return entities.Select(x => x.ToRunStatisticsModel()).ToList().AsReadOnly();
    }

    public static Graph ToGraphEntity<T>(this CreateGraphRequest<T> request)
        where T : IVertex
    {
        return new()
        {
            Name = request.Name,
            Neighborhood = request.Neighborhood,
            SmoothLevel = request.SmoothLevel,
            Dimensions = string.Join(",", request.Graph.DimensionsSizes),
            Status = request.Status,
            UpperValueRange = request.Graph.CostRange.UpperValueOfRange,
            LowerValueRange = request.Graph.CostRange.LowerValueOfRange
        };
    }

    public static Vertex ToVertexEntity<T>(this T vertex)
        where T : IVertex, IEntity<long>
    {
        return new()
        {
            Id = vertex.Id,
            Coordinates = string.Join(",", vertex.Position),
            Cost = vertex.Cost.CurrentCost,
            IsObstacle = vertex.IsObstacle
        };
    }

    public static IReadOnlyCollection<Vertex> ToVertexEntities<T>(this IEnumerable<T> vertices)
        where T : IVertex, IEntity<long>
    {
        return vertices.Select(x => x.ToVertexEntity()).ToList().AsReadOnly();
    }

    public static GraphInformationModel ToGraphInformationModel(this Graph graph)
    {
        return new()
        {
            Name = graph.Name,
            Neighborhood = graph.Neighborhood,
            Dimensions = [.. graph.Dimensions.Split(",").Select(int.Parse)],
            Id = graph.Id,
            SmoothLevel = graph.SmoothLevel,
            Status = graph.Status,
            CostRange = (graph.LowerValueRange, graph.UpperValueRange)
        };
    }

    public static IReadOnlyCollection<GraphInformationModel> ToInformationModels(this IEnumerable<Graph> entities)
    {
        return entities.Select(x => x.ToGraphInformationModel()).ToList().AsReadOnly();
    }

    public static Graph ToGraphEntity(this GraphInformationModel model)
    {
        return new()
        {
            Name = model.Name,
            Neighborhood = model.Neighborhood,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            Dimensions = string.Join(",", model.Dimensions),
            Id = model.Id,
            LowerValueRange = model.CostRange.LowerValueOfRange,
            UpperValueRange = model.CostRange.UpperValueOfRange
        };
    }

    
}
