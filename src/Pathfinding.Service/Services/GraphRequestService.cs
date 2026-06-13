using Pathfinding.Data;
using Pathfinding.Data.InMemory;
using Pathfinding.Domain;
using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Services;

public sealed class GraphRequestService<T>(IUnitOfWorkFactory factory) : IGraphRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
    public GraphRequestService() : this(new InMemoryUnitOfWorkFactory())
    {

    }

    public Task<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(
        IReadOnlyCollection<PathfindingHistorySerializationModel> request,
        CancellationToken token = default)
    {
        return factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var models = new List<PathfindingHistoryModel<T>>();
            foreach (var history in request)
            {
                var graphModel = history.Graph;
                var vertices = history.Vertices.ToVertices<T>();
                var dimensions = graphModel.DimensionSizes;
                var graph = new Graph<T>(vertices, dimensions)
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
                var rangeVertices = range
                    .Select((x, i) => (Order: i, Vertex: graph.Get(x)))
                    .ToList();
                var entities = rangeVertices.Select(x => new PathfindingRange
                {
                    GraphId = model.Id,
                    VertexId = x.Vertex.Id,
                    Order = x.Order
                }).ToArray();
                await unitOfWork.RangeRepository
                    .CreateAsync(entities, t)
                    .ConfigureAwait(false);
                models.Add(new PathfindingHistoryModel<T>
                {
                    Graph = new GraphModel<T>
                    {
                        Id = model.Id,
                        DimensionSizes = dimensions,
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
        }, token).AsTask();
    }

    public Task<bool> UpdateVerticesAsync(
        UpdateVerticesRequest<T> request,
        CancellationToken token = default)
    {
        return ExecuteAsync((unitOfWork, t) =>
        {
            var vertices = request.Vertices
                .ToVertexEntities()
                .ForEach(x => x.GraphId = request.GraphId);
            return unitOfWork.VerticesRepository.UpdateVerticesAsync([.. vertices], t);
        }, token);
    }

    public Task<GraphModel<T>> ReadGraphAsync(
        int graphId,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unitOfWork, t) =>
        {
            var graphEntity = await unitOfWork.GraphRepository
                .ReadAsync(graphId, t)
                .ConfigureAwait(false);
            var vertices = await unitOfWork.VerticesRepository
                .ReadVerticesByGraphIdAsync(graphId)
                .Select(x => x.ToVertex<T>())
                .ToListAsync(t)
                .ConfigureAwait(false);
            return new GraphModel<T>
            {
                Vertices = vertices,
                DimensionSizes = [.. graphEntity.Dimensions.Split(",").Select(int.Parse)],
                Id = graphEntity.Id,
                Name = graphEntity.Name,
                Neighborhood = graphEntity.Neighborhood,
                SmoothLevel = graphEntity.SmoothLevel,
                Status = graphEntity.Status,
                CostRange = (graphEntity.LowerValueRange, graphEntity.UpperValueRange)
            };
        }, token);
    }

    public Task<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default)
    {
        return factory.TransactionAsync(
            (unit, t) => unit.CreateGraphAsyncInternal(graph, t),
            token).AsTask();
    }

    public Task<PathfindingHistoriesSerializationModel> ReadSerializationHistoriesAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork
                .ReadGraphsInternalAsync<T>(graphIds, t)
                .ConfigureAwait(false);
            var ranges = await unitOfWork
                .ReadRangesAsyncInternal(graphIds, t)
                .ConfigureAwait(false);
            var statistics = (await unitOfWork.StatisticsRepository
                .ReadByGraphIdsAsync(graphIds)
                .ToArrayAsync(t)
                .ConfigureAwait(false))
                .GroupBy(x => x.GraphId, x => x.ToSerializationModel())
                .ToDictionary(x => x.Key, x => x.ToArray());

            var histories = graphs
                .Select(graph => ToSerializationHistory(
                    graph,
                    ranges.GetValueOrDefault(graph.Id, []),
                    statistics.GetValueOrDefault(graph.Id, [])))
                .ToList();

            return new PathfindingHistoriesSerializationModel { Histories = histories };
        }, token);
    }

    public Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork
                .ReadGraphsInternalAsync<T>(graphIds, t)
                .ConfigureAwait(false);
            var histories = graphs
                .Select(graph =>
                {
                    graph.Status = GraphStatuses.Editable;
                    return ToSerializationHistory(graph, [], []);
                })
                .ToList();

            return new PathfindingHistoriesSerializationModel { Histories = histories };
        }, token);
    }

    public Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsWithRangeAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork
                .ReadGraphsInternalAsync<T>(graphIds, t)
                .ConfigureAwait(false);
            var ranges = await unitOfWork
                .ReadRangesAsyncInternal(graphIds, t)
                .ConfigureAwait(false);
            var histories = graphs
                .Select(graph =>
                {
                    graph.Status = GraphStatuses.Editable;
                    return ToSerializationHistory(graph, ranges.GetValueOrDefault(graph.Id, []), []);
                })
                .ToList();

            return new PathfindingHistoriesSerializationModel { Histories = histories };
        }, token);
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IUnitOfWork, CancellationToken, Task<TResult>> action,
        CancellationToken token)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        return await action(unit, token).ConfigureAwait(false);
    }

    private static PathfindingHistorySerializationModel ToSerializationHistory(
        GraphModel<T> graph,
        IReadOnlyCollection<PathfindingRangeModel> range,
        IReadOnlyCollection<RunStatisticsSerializationModel> statistics)
    {
        var verticesById = graph.Vertices.ToDictionary(x => x.Id, x => x.Position);
        var coordinates = range
            .Select(x => verticesById.TryGetValue(x.VertexId, out var coordinate)
                ? new CoordinateModel { Coordinate = coordinate }
                : null)
            .OfType<CoordinateModel>()
            .ToList();

        return new()
        {
            Graph = graph.ToSerializationModel(),
            Vertices = graph.Vertices.ToSerializationModels(),
            Statistics = statistics,
            Range = coordinates
        };
    }
}
