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

    public async Task<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(
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
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> UpdateVerticesAsync(
        UpdateVerticesRequest<T> request,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var vertices = request.Vertices
            .ToVertexEntities()
            .ForEach(x => x.GraphId = request.GraphId);
        return await unitOfWork.VerticesRepository
            .UpdateVerticesAsync([.. vertices], token)
            .ConfigureAwait(false);
    }

    public async Task<GraphModel<T>> ReadGraphAsync(
        int graphId,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphEntity = await unitOfWork.GraphRepository
            .ReadAsync(graphId, token)
            .ConfigureAwait(false);
        var vertices = await unitOfWork.VerticesRepository
            .ReadVerticesByGraphIdAsync(graphId)
            .Select(x => x.ToVertex<T>())
            .ToListAsync(token)
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
    }

    public async Task<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            return await unit
                .CreateGraphAsyncInternal(graph, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationHistoriesAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await unitOfWork
            .ReadGraphsInternalAsync<T>(graphIds, token)
            .ConfigureAwait(false);
        var ranges = await unitOfWork
            .ReadRangesAsyncInternal(graphIds, token)
            .ConfigureAwait(false);
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

        return new PathfindingHistoriesSerializationModel { Histories = histories };
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await unitOfWork
            .ReadGraphsInternalAsync<T>(graphIds, token)
            .ConfigureAwait(false);
        var histories = graphs
            .Select(graph =>
            {
                graph.Status = GraphStatuses.Editable;
                return ToSerializationHistory(graph, [], []);
            })
            .ToList();

        return new PathfindingHistoriesSerializationModel { Histories = histories };
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsWithRangeAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
        var graphs = await unitOfWork
            .ReadGraphsInternalAsync<T>(graphIds, token)
            .ConfigureAwait(false);
        var ranges = await unitOfWork
            .ReadRangesAsyncInternal(graphIds, token)
            .ConfigureAwait(false);
        var histories = graphs
            .Select(graph =>
            {
                graph.Status = GraphStatuses.Editable;
                return ToSerializationHistory(graph, ranges.GetValueOrDefault(graph.Id, []), []);
            })
            .ToList();

        return new PathfindingHistoriesSerializationModel { Histories = histories };
    }

    private static PathfindingHistorySerializationModel ToSerializationHistory(
        GraphModel<T> graph,
        IReadOnlyCollection<PathfindingRangeModel> range,
        IReadOnlyCollection<RunStatisticsSerializationModel> statistics)
    {
        var verticesById = graph.Vertices.ToDictionary(x => x.Id, x => x.Position);
        var coordinates = range
            .Where(x => verticesById.ContainsKey(x.VertexId))
            .Select(x => new CoordinateModel { Coordinate = verticesById[x.VertexId] })
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
