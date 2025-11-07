using Pathfinding.Domain.Core;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Data.InMemory;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business;

public sealed class GraphRequestService<T>(IUnitOfWorkFactory factory) : IGraphRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
    public GraphRequestService() : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public async Task<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(
        IEnumerable<PathfindingHistorySerializationModel> request,
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
                var graph = new Graph<T>(vertices, dimensions);
                var createGraphRequest = new CreateGraphRequest<T>
                {
                    Graph = graph,
                    Neighborhood = graphModel.Neighborhood,
                    SmoothLevel = graphModel.SmoothLevel,
                    Name = graphModel.Name,
                    Status = graphModel.Status
                };
                var model = await RequestServiceHelpers.CreateGraphAsyncInternal(unitOfWork, createGraphRequest, t)
                    .ConfigureAwait(false);
                var statistics = history.Statistics.ToStatistics();
                statistics.ForEach(x => x.GraphId = model.Id);
                await unitOfWork.StatisticsRepository.CreateAsync(statistics, t).ConfigureAwait(false);
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
                    Order = x.Order,
                    IsSource = x.Order == 0,
                    IsTarget = x.Order == rangeVertices.Count - 1 && rangeVertices.Count > 1
                }).ToReadOnly();
                await unitOfWork.RangeRepository.CreateAsync(entities, t).ConfigureAwait(false);
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
                        Status = graphModel.Status
                    },
                    Statistics = statistics.ToRunStatisticsModels(),
                    Range = range
                });
            }

            return models.AsReadOnly();
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> UpdateVerticesAsync(UpdateVerticesRequest<T> request, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var vertices = request.Vertices
                .ToVertexEntities()
                .ForEach(x => x.GraphId = request.GraphId);
            return await unitOfWork.VerticesRepository
                .UpdateVerticesAsync([.. vertices], t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<GraphModel<T>> ReadGraphAsync(int graphId, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t)
                => await RequestServiceHelpers.ReadGraphInternalAsync<T>(unitOfWork, graphId, t).ConfigureAwait(false), token)
            .ConfigureAwait(false);
    }

    public async Task<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t)
                => await RequestServiceHelpers.CreateGraphAsyncInternal(unit, graph, t).ConfigureAwait(false), token)
            .ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationHistoriesAsync(
        IEnumerable<int> graphIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graphId in graphIds)
            {
                var graph = await RequestServiceHelpers.ReadGraphInternalAsync<T>(unitOfWork, graphId, t)
                    .ConfigureAwait(false);
                var range = await RequestServiceHelpers.ReadRangeAsyncInternal<T>(unitOfWork, graphId, t)
                    .ConfigureAwait(false);
                var statistics = await unitOfWork.StatisticsRepository
                    .ReadByGraphIdAsync(graphId)
                    .ToListAsync(t)
                    .ConfigureAwait(false);
                result.Add(new PathfindingHistorySerializationModel
                {
                    Graph = graph.ToSerializationModel(),
                    Vertices = graph.Vertices.ToSerializationModels(),
                    Statistics = statistics.ToSerializationModels(),
                    Range = range.ToCoordinates()
                });
            }

            return new PathfindingHistoriesSerializationModel { Histories = result };
        }, token).ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(
        IEnumerable<int> graphIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graphId in graphIds)
            {
                var graph = await RequestServiceHelpers.ReadGraphInternalAsync<T>(unitOfWork, graphId, t)
                    .ConfigureAwait(false);
                graph.Status = GraphStatuses.Editable;
                result.Add(new PathfindingHistorySerializationModel
                {
                    Graph = graph.ToSerializationModel(),
                    Vertices = graph.Vertices.ToSerializationModels(),
                    Statistics = [],
                    Range = []
                });
            }

            return new PathfindingHistoriesSerializationModel { Histories = result };
        }, token).ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsWithRangeAsync(
        IEnumerable<int> graphIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graphId in graphIds)
            {
                var graph = await RequestServiceHelpers.ReadGraphInternalAsync<T>(unitOfWork, graphId, t)
                    .ConfigureAwait(false);
                graph.Status = GraphStatuses.Editable;
                var range = await RequestServiceHelpers.ReadRangeAsyncInternal<T>(unitOfWork, graphId, t)
                    .ConfigureAwait(false);
                result.Add(new PathfindingHistorySerializationModel
                {
                    Graph = graph.ToSerializationModel(),
                    Vertices = graph.Vertices.ToSerializationModels(),
                    Statistics = [],
                    Range = range.ToCoordinates()
                });
            }

            return new PathfindingHistoriesSerializationModel { Histories = result };
        }, token).ConfigureAwait(false);
    }
}
