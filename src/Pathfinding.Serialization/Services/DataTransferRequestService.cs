using Pathfinding.Data;
using Pathfinding.Domain;
using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Serialization.Extensions;
using Pathfinding.Serialization.Models;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Serialization.Services;

public sealed class DataTransferRequestService<T>(IUnitOfWorkFactory factory) 
    : IDataTransferRequestService<T> where T : IVertex, IEntity<long>, new()
{
    public async ValueTask<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(
        IReadOnlyCollection<PathfindingHistorySerializationModel> request,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var models = new List<PathfindingHistoryModel<T>>();
            foreach (var history in request)
            {
                var graphModel = history.Graph;
                var vertices = history.Vertices.ToVertices<T>();
                var graph = new Graph<T>(vertices, graphModel.DimensionSizes)
                {
                    CostRange = graphModel.CostRange
                };
                var createGraphRequest = new CreateGraphRequest<T>
                {
                    Graph = graph,
                    Neighborhood = graphModel.Neighborhood,
                    SmoothLevel = graphModel.SmoothLevel,
                    Name = graphModel.Name,
                    Status = graphModel.Status
                };
                var model = await unitOfWork
                    .CreateGraphAsyncInternal(createGraphRequest, t)
                    .ConfigureAwait(false);
                var statistics = history.Statistics.ToStatistics();
                statistics.ForEach(x => x.GraphId = model.Id);
                await unitOfWork.StatisticsRepository
                    .CreateAsync(statistics, t)
                    .ConfigureAwait(false);
                var range = history.Range
                    .Select(x => new Coordinate(x.Coordinate))
                    .ToList();
                await unitOfWork.RangeRepository
                    .CreateAsync([.. range
                    .Select((x, i) => new PathfindingRange
                    {
                        GraphId = model.Id,
                        VertexId = graph.Get(x).Id,
                        Order = i
                    })], t)
                    .ConfigureAwait(false);
                models.Add(new PathfindingHistoryModel<T>
                {
                    Graph = new GraphModel<T>
                    {
                        Id = model.Id,
                        DimensionSizes = graphModel.DimensionSizes,
                        Vertices = vertices,
                        Neighborhood = graphModel.Neighborhood,
                        SmoothLevel = graphModel.SmoothLevel,
                        Name = graphModel.Name,
                        Status = graphModel.Status,
                        CostRange = graphModel.CostRange
                    },
                    Statistics = statistics.ToRunStatisticsModels(),
                    Range = range
                });
            }
            return models.AsReadOnly();
        }, token).ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(
        IReadOnlyCollection<int> graphIds, 
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await ReadGraphsInternalAsync(unitOfWork, graphIds, token).ConfigureAwait(false);
        var histories = graphs
            .Select(graph =>
            {
                graph.Status = GraphStatuses.Editable;
                return ToSerializationHistory(graph, [], []);
            })
            .ToList();

        return new() { Histories = histories };
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsWithRangeAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await ReadGraphsInternalAsync(unitOfWork, graphIds, token).ConfigureAwait(false);
        var ranges = await ReadRangesAsyncInternal(unitOfWork, graphIds, token).ConfigureAwait(false);
        var histories = graphs
            .Select(graph =>
            {
                graph.Status = GraphStatuses.Editable;
                return ToSerializationHistory(graph, ranges.GetValueOrDefault(graph.Id, []), []);
            })
            .ToList();

        return new () { Histories = histories };
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationHistoriesAsync(
        IReadOnlyCollection<int> graphIds, 
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await ReadGraphsInternalAsync(unitOfWork, graphIds, token).ConfigureAwait(false);
        var ranges = await ReadRangesAsyncInternal(unitOfWork, graphIds, token).ConfigureAwait(false);
        var statistics = (await unitOfWork.StatisticsRepository
            .ReadByGraphIdsAsync(graphIds)
            .ToArrayAsync(token)
            .ConfigureAwait(false))
            .GroupBy(x => x.GraphId, x => x.ToSerializationModel())
            .ToDictionary(x => x.Key, x => x.ToArray());

        var histories = graphs
            .Select(graph => ToSerializationHistory(
                graph,
                ranges.GetValueOrDefault(graph.Id, []),
                statistics.GetValueOrDefault(graph.Id, [])))
            .ToList();

        return new () { Histories = histories };
    }

    private static PathfindingHistorySerializationModel ToSerializationHistory(
        GraphModel<T> graph,
        IReadOnlyCollection<PathfindingRangeModel> range,
        IReadOnlyCollection<RunStatisticsSerializationModel> statistics)
    {
        var verticesById = graph.Vertices.ToDictionary(x => x.Id, x => x.Position);
        var coordinates = range
            .Select(x => verticesById.TryGetValue(x.VertexId, out var coordinate)
                ? new CoordinateSerializationModel { Coordinate = coordinate }
                : null)
            .OfType<CoordinateSerializationModel>()
            .ToList();

        return new()
        {
            Graph = graph.ToSerializationModel(),
            Vertices = graph.Vertices.ToSerializationModels(),
            Statistics = statistics,
            Range = coordinates
        };
    }

    private static async Task<IReadOnlyDictionary<int, IReadOnlyCollection<PathfindingRangeModel>>> ReadRangesAsyncInternal(
        IUnitOfWork unit, 
        IReadOnlyCollection<int> graphIds, 
        CancellationToken token = default)
    {
        return (await unit.RangeRepository
            .ReadByGraphIdsAsync(graphIds)
            .ToArrayAsync(token))
            .GroupBy(x => x.GraphId)
            .ToDictionary(
                x => x.Key,
                x => (IReadOnlyCollection<PathfindingRangeModel>)[.. x
                    .OrderBy(y => y.Order)
                    .Select(y => y.ToRangeModel())]);
    }

    private static async Task<IReadOnlyCollection<GraphModel<T>>> ReadGraphsInternalAsync(
        IUnitOfWork unit, 
        IReadOnlyCollection<int> graphIds, 
        CancellationToken token = default)
    {
        var graphs = await unit.GraphRepository.ReadAsync(graphIds)
            .ToArrayAsync(token)
            .ConfigureAwait(false);
        var vertices = (await unit.VerticesRepository.ReadVerticesByGraphIdsAsync(graphIds)
            .ToArrayAsync(token)
            .ConfigureAwait(false))
            .GroupBy(x => x.GraphId, x => x.ToVertex<T>())
            .ToDictionary(x => x.Key, x => x.ToArray());
        var models = new List<GraphModel<T>>();
        foreach (var graph in graphs)
        {
            models.Add(new()
            {
                DimensionSizes = [.. graph.Dimensions.Split(",").Select(int.Parse)],
                Id = graph.Id,
                Name = graph.Name,
                Neighborhood = graph.Neighborhood,
                SmoothLevel = graph.SmoothLevel,
                Status = graph.Status,
                CostRange = (graph.LowerValueRange, graph.UpperValueRange),
                Vertices = vertices[graph.Id]
            });
        }
        return models;
    }
}
