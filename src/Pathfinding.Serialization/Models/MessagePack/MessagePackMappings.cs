using Newtonsoft.Json;

namespace Pathfinding.Serialization.Models.MessagePack;

internal static class MessagePackMappings
{
    private static MessagePackVertex ToMessagePackVertex(this VertexSerializationModel model, int graphId)
    {
        return new()
        {
            GraphId = graphId,
            Cost = model.Cost.Cost,
            IsObstacle = model.IsObstacle,
            Coordinate = JsonConvert.SerializeObject(model.Position.Coordinate)
        };
    }

    private static List<MessagePackVertex> ToMessagePackVertices(this IEnumerable<VertexSerializationModel> models, int graphId)
    {
        return [.. models.Select(x => x.ToMessagePackVertex(graphId))];
    }

    private static MessagePackGraph ToMessagePackGraph(this GraphSerializationModel model, int graphId)
    {
        return new()
        {
            Id = graphId,
            Name = model.Name,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            Neighborhood = model.Neighborhood,
            DimensionSizes = JsonConvert.SerializeObject(model.DimensionSizes),
            UpperValueOfRange = model.CostRange.UpperValueOfRange,
            LowerValueOfRange = model.CostRange.LowerValueOfRange
        };
    }

    private static MessagePackStatistics ToMessagePackStatistics(this RunStatisticsSerializationModel model, int graphId)
    {
        return new()
        {
            GraphId = graphId,
            Algorithm = model.Algorithm,
            Heuristics = model.Heuristics,
            StepRule = model.StepRule,
            Weight = model.Weight,
            Elapsed = model.Elapsed.TotalMilliseconds,
            Visited = model.Visited,
            ResultStatus = model.ResultStatus,
            Cost = model.Cost,
            Steps = model.Steps
        };
    }

    private static List<MessagePackStatistics> ToMessagePackStatistics(this IEnumerable<RunStatisticsSerializationModel> models, int graphId)
    {
        return [.. models.Select(x => x.ToMessagePackStatistics(graphId))];
    }

    private static MessagePackRange ToMessagePackRange(this CoordinateSerializationModel model, int graphId)
    {
        return new() { Coordinate = JsonConvert.SerializeObject(model.Coordinate), GraphId = graphId };
    }

    private static List<MessagePackRange> ToMessagePackRanges(this IEnumerable<CoordinateSerializationModel> models, int graphId)
    {
        return [.. models.Select(x => x.ToMessagePackRange(graphId))];
    }

    private static GraphSerializationModel ToGraph(this MessagePackGraph model)
    {
        return new()
        {
            Name = model.Name,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            Neighborhood = model.Neighborhood,
            DimensionSizes = JsonConvert.DeserializeObject<int[]>(model.DimensionSizes),
            CostRange = new(model.LowerValueOfRange, model.UpperValueOfRange)
        };
    }

    private static VertexSerializationModel ToVertex(this MessagePackVertex model)
    {
        return new()
        {
            Cost = new() { Cost = model.Cost },
            IsObstacle = model.IsObstacle,
            Position = new()
            {
                Coordinate = JsonConvert.DeserializeObject<int[]>(model.Coordinate)
            }
        };
    }

    private static List<VertexSerializationModel> ToVertices(this IEnumerable<MessagePackVertex> models)
    {
        return [.. models.Select(x => x.ToVertex())];
    }

    private static RunStatisticsSerializationModel ToStatistics(this MessagePackStatistics model)
    {
        return new()
        {
            Algorithm = model.Algorithm,
            Heuristics = model.Heuristics,
            StepRule = model.StepRule,
            Weight = model.Weight,
            Elapsed = TimeSpan.FromMilliseconds(model.Elapsed),
            Visited = model.Visited,
            ResultStatus = model.ResultStatus,
            Cost = model.Cost,
            Steps = model.Steps
        };
    }

    private static List<RunStatisticsSerializationModel> ToStatistics(this IEnumerable<MessagePackStatistics> models)
    {
        return [.. models.Select(x => x.ToStatistics())];
    }

    private static CoordinateSerializationModel ToRange(this MessagePackRange model)
    {
        return new() { Coordinate = JsonConvert.DeserializeObject<int[]>(model.Coordinate) };
    }

    private static List<CoordinateSerializationModel> ToRanges(this IEnumerable<MessagePackRange> models)
    {
        return [.. models.Select(x => x.ToRange())];
    }

    public static List<MessagePackHistory> ToMessagePackHistory(this IEnumerable<PathfindingHistorySerializationModel> models)
    {
        return [.. models.Select((x, i) => new MessagePackHistory
        {
            Graph = x.Graph.ToMessagePackGraph(i + 1),
            Vertices = x.Vertices.ToMessagePackVertices(i + 1),
            Statistics = x.Statistics.ToMessagePackStatistics(i + 1),
            Range = x.Range.ToMessagePackRanges(i + 1)
        })];
    }

    public static List<PathfindingHistorySerializationModel> ToHistory(this
        List<MessagePackHistory> models)
    {
        var vertices = models.SelectMany(x => x.Vertices).GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var statistics = models.SelectMany(x => x.Statistics).GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var ranges = models.SelectMany(x => x.Range).GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.ToList());
        return [.. models.Select(x => x.Graph).Select(x => new PathfindingHistorySerializationModel
        {
            Graph = x.ToGraph(),
            Vertices = vertices.GetValueOrDefault(x.Id, []).ToVertices(),
            Statistics = statistics.GetValueOrDefault(x.Id, []).ToStatistics(),
            Range = ranges.GetValueOrDefault(x.Id, []).ToRanges()
        })];
    }
}
