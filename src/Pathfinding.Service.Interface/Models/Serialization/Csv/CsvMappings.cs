using Newtonsoft.Json;
using Pathfinding.Service.Interface.Models.Undefined;

namespace Pathfinding.Service.Interface.Models.Serialization.Csv;

internal static class CsvMappings
{
    private static CsvVertex ToVertex(this VertexSerializationModel model, int graphId)
    {
        return new CsvVertex()
        {
            GraphId = graphId,
            LowerValueOfRange = model.Cost.LowerValueOfRange,
            UpperValueOfRange = model.Cost.UpperValueOfRange,
            Cost = model.Cost.Cost,
            IsObstacle = model.IsObstacle,
            Coordinate = JsonConvert.SerializeObject(model.Position.Coordinate)
        };
    }

    private static List<CsvVertex> ToVertices(this IEnumerable<VertexSerializationModel> models, int graphId)
    {
        return [.. models.Select(x => x.ToVertex(graphId))];
    }

    private static CsvGraph ToGraph(this GraphSerializationModel model, int graphId)
    {
        return new()
        {
            Id = graphId,
            Name = model.Name,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            Neighborhood = model.Neighborhood,
            DimensionSizes = JsonConvert.SerializeObject(model.DimensionSizes)
        };
    }

    private static CsvStatistics ToStatistics(this RunStatisticsSerializationModel model, int graphId)
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

    private static List<CsvStatistics> ToStatisics(this IEnumerable<RunStatisticsSerializationModel> models, int graphId)
    {
        return [.. models.Select(x => x.ToStatistics(graphId))];
    }

    private static CsvRange ToRange(this CoordinateModel model, int graphId)
    {
        return new() { Coordinate = JsonConvert.SerializeObject(model.Coordinate), GraphId = graphId };
    }

    private static List<CsvRange> ToRanges(this IEnumerable<CoordinateModel> models, int graphId)
    {
        return [.. models.Select(x => x.ToRange(graphId))];
    }

    private static GraphSerializationModel ToGraph(this CsvGraph model)
    {
        return new()
        {
            Name = model.Name,
            SmoothLevel = model.SmoothLevel,
            Status = model.Status,
            Neighborhood = model.Neighborhood,
            DimensionSizes = JsonConvert.DeserializeObject<int[]>(model.DimensionSizes)
        };
    }

    private static VertexSerializationModel ToVertex(this CsvVertex model)
    {
        return new VertexSerializationModel()
        {
            Cost = new VertexCostModel()
            {
                Cost = model.Cost,
                UpperValueOfRange = model.UpperValueOfRange,
                LowerValueOfRange = model.LowerValueOfRange
            },
            IsObstacle = model.IsObstacle,
            Position = new CoordinateModel() 
            { 
                Coordinate = JsonConvert.DeserializeObject<int[]>(model.Coordinate) 
            }
        };
    }

    private static List<VertexSerializationModel> ToVertices(this IEnumerable<CsvVertex> models)
    {
        return [.. models.Select(x => x.ToVertex())];
    }

    private static RunStatisticsSerializationModel ToStatistics(this CsvStatistics model)
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

    private static List<RunStatisticsSerializationModel> ToStatistics(this IEnumerable<CsvStatistics> models)
    {
        return [.. models.Select(x => x.ToStatistics())];
    }

    private static CoordinateModel ToRange(this CsvRange model)
    {
        return new() { Coordinate = JsonConvert.DeserializeObject<int[]>(model.Coordinate) };
    }

    private static List<CoordinateModel> ToRanges(this IEnumerable<CsvRange> models)
    {
        return [.. models.Select(x => x.ToRange())];
    }

    public static List<CsvHistory> ToHistory(this IEnumerable<PathfindingHistorySerializationModel> models)
    {
        return [.. models.Select((x, i) => new CsvHistory()
        {
            Graph = x.Graph.ToGraph(i + 1),
            Vertices = x.Vertices.ToVertices(i + 1),
            Statistics = x.Statistics.ToStatisics(i + 1),
            Range = x.Range.ToRanges(i + 1)
        })];
    }

    public static List<PathfindingHistorySerializationModel> ToHistory(this 
        (IEnumerable<CsvGraph> Graphs,
        IEnumerable<CsvVertex> Vertices,
        IEnumerable<CsvStatistics> Statistics,
        IEnumerable<CsvRange> Ranges) models)
    {
        var vertices = models.Vertices.GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var statistics = models.Statistics.GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var ranges = models.Ranges.GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.ToList());
        return [.. models.Graphs.Select(x => new PathfindingHistorySerializationModel()
        {
            Graph = x.ToGraph(),
            Vertices = vertices.GetValueOrDefault(x.Id, []).ToVertices(),
            Statistics = statistics.GetValueOrDefault(x.Id, []).ToStatistics(),
            Range = ranges.GetValueOrDefault(x.Id, []).ToRanges()
        })];
    }
}