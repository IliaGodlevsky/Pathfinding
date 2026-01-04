using Pathfinding.Domain.Core;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Extensions;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Services;

public sealed class GraphRequestService<T>(IUnitOfWorkFactory factory) : IGraphRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
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
                var graph = new Graph<T>(vertices, dimensions);
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
                    Order = x.Order,
                    IsSource = x.Order == 0,
                    IsTarget = x.Order == rangeVertices.Count - 1 && rangeVertices.Count > 1
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
                        Status = graphModel.Status
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

    public async Task<GraphModel<T>> ReadGraphAsync(
        int graphId, 
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var graphEntity = await unitOfWork.GraphRepository
                .ReadAsync(graphId, token).ConfigureAwait(false);
            var vertices = await unitOfWork.VerticesRepository
                .ReadVerticesByGraphIdAsync(graphId)
                .Select(x => x.ToVertex<T>())
                .ToListAsync(token)
                .ConfigureAwait(false);
            return new GraphModel<T>
            {
                Vertices = vertices,
                DimensionSizes = graphEntity.Dimensions.ToDimensionSizes(),
                Id = graphEntity.Id,
                Name = graphEntity.Name,
                Neighborhood = graphEntity.Neighborhood,
                SmoothLevel = graphEntity.SmoothLevel,
                Status = graphEntity.Status
            };
        }, token).ConfigureAwait(false);
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
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork
                .ReadGraphsInternalAsync<T>(graphIds, t)
                .ConfigureAwait(false);
            var ranges = await unitOfWork
                .ReadRangesAsyncInternal(graphIds, t)
                .ConfigureAwait(false);
            var statisitics = (await unitOfWork.StatisticsRepository
                .ReadByGraphIdsAsync(graphIds)
                .ToArrayAsync(t))
                .GroupBy(x => x.GraphId, x => x.ToSerializationModel())
                .ToDictionary(x => x.Key, x => x.ToArray());
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graph in graphs)
            {
                var graphDict = graph.Vertices.ToDictionary(x => x.Id, x => x.Position.ToArray());
                var range = ranges[graph.Id].Select(x => graphDict[x.VertexId])
                    .Select(x => new CoordinateModel() { Coordinate = x })
                    .ToList();
                result.Add(new PathfindingHistorySerializationModel
                {
                    Graph = graph.ToSerializationModel(),
                    Vertices = graph.Vertices.ToSerializationModels(),
                    Statistics = statisitics[graph.Id],
                    Range = range
                });
            }

            return new PathfindingHistoriesSerializationModel { Histories = result };
        }, token).ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var graphs = await unitOfWork
                .ReadGraphsInternalAsync<T>(graphIds, t)
                .ConfigureAwait(false);
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graph in graphs)
            {
                graph.Status = GraphStatuses.Editable;
                result.Add(new PathfindingHistorySerializationModel()
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
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            var graphs = await unitOfWork
                .ReadGraphsInternalAsync<T>(graphIds, t)
                .ConfigureAwait(false);
            var ranges = await unitOfWork
                .ReadRangesAsyncInternal(graphIds, t)
                .ConfigureAwait(false);
            foreach (var graph in graphs)
            {
                var graphDictionary = graph.Vertices
                    .ToDictionary(x => x.Id, x => x.Position.ToArray());
                graph.Status = GraphStatuses.Editable;
                var coordinates = ranges[graph.Id]
                    .Select(x => new CoordinateModel { Coordinate = graphDictionary[x.VertexId] })
                    .ToList();
                result.Add(new PathfindingHistorySerializationModel
                {
                    Graph = graph.ToSerializationModel(),
                    Vertices = graph.Vertices.ToSerializationModels(),
                    Statistics = [],
                    Range = coordinates
                });
            }

            return new PathfindingHistoriesSerializationModel { Histories = result };
        }, token).ConfigureAwait(false);
    }
}
