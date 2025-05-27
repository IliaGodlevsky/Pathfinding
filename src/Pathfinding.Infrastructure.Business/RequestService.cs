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

public sealed class RequestService<T>(IUnitOfWorkFactory factory) : IRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
    public RequestService()
        : this(new InMemoryUnitOfWorkFactory())
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
                var model = await CreateGraphAsyncInternal(unitOfWork, createGraphRequest, t).ConfigureAwait(false);
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
                models.Add(new()
                {
                    Graph = new()
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

    public async Task<bool> DeleteRangeAsync(IEnumerable<T> request,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var verticesIds = request.Select(x => x.Id).ToList();
            return await unitOfWork.RangeRepository.DeleteByVerticesIdsAsync(verticesIds, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> DeleteRangeAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t)
            => await unitOfWork.RangeRepository.DeleteByGraphIdAsync(graphId, t)
            .ConfigureAwait(false), token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<GraphInformationModel>> ReadAllGraphInfoAsync(CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = await unitOfWork.GraphRepository
                .GetAll()
                .ToListAsync(t)
                .ConfigureAwait(false);
            var ids = result.Select(x => x.Id).ToHashSet();
            var obstaclesCount = await unitOfWork.GraphRepository.ReadObstaclesCountAsync(ids, t)
                .ConfigureAwait(false);
            var infos = result.ToInformationModels();
            infos.ForEach(x => x.ObstaclesCount = obstaclesCount[x.Id]);
            return infos;
        }, token).ConfigureAwait(false);
    }

    public async Task<GraphModel<T>> ReadGraphAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t)
            => await ReadGraphInternalAsync(unitOfWork, graphId, t).ConfigureAwait(false), token)
            .ConfigureAwait(false);
    }

    public async Task<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t)
            => await CreateGraphAsyncInternal(unit, graph, t).ConfigureAwait(false), token)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t)
            => await ReadRangeAsyncInternal(unit, graphId, t).ConfigureAwait(false), token)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByGraphIdAsync(graphId)
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationHistoriesAsync(IEnumerable<int> graphIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graphId in graphIds)
            {
                var graph = await ReadGraphInternalAsync(unitOfWork, graphId, t).ConfigureAwait(false);
                var range = await ReadRangeAsyncInternal(unitOfWork, graphId, t).ConfigureAwait(false);
                var statistics = await unitOfWork.StatisticsRepository
                    .ReadByGraphIdAsync(graphId)
                    .ToListAsync(t)
                    .ConfigureAwait(false);
                result.Add(new()
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

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(IEnumerable<int> graphIds, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graphId in graphIds)
            {
                var graph = await ReadGraphInternalAsync(unitOfWork, graphId, t)
                    .ConfigureAwait(false);
                graph.Status = GraphStatuses.Editable;
                result.Add(new()
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

    public async Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsWithRangeAsync(IEnumerable<int> graphIds, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var result = new List<PathfindingHistorySerializationModel>();
            foreach (var graphId in graphIds)
            {
                var graph = await ReadGraphInternalAsync(unitOfWork, graphId, t).ConfigureAwait(false);
                graph.Status = GraphStatuses.Editable;
                var range = await ReadRangeAsyncInternal(unitOfWork, graphId, t).ConfigureAwait(false);
                result.Add(new()
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

    public async Task<bool> UpdateVerticesAsync(UpdateVerticesRequest<T> request,
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

    public async Task<bool> DeleteGraphsAsync(IEnumerable<int> ids,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t)
            => await unitOfWork.GraphRepository.DeleteAsync([.. ids], t)
                .ConfigureAwait(false), token)
            .ConfigureAwait(false);
    }

    public async Task<bool> UpdateStatisticsAsync(IEnumerable<RunStatisticsModel> models,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var entities = models.ToStatistics();
            return await unit.StatisticsRepository.UpdateAsync(entities, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> DeleteRunsAsync(IEnumerable<int> runIds, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) => await unit.StatisticsRepository
            .DeleteByIdsAsync([.. runIds], t)
            .ConfigureAwait(false), token).ConfigureAwait(false);
    }

    public async Task<bool> CreatePathfindingVertexAsync(int graphId,
        long vertexId, int index, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var range = await unit.RangeRepository
                .ReadByGraphIdAsync(graphId)
                .ToListAsync(t)
                .ConfigureAwait(false);
            var pathfindingRange = new PathfindingRange
            {
                GraphId = graphId,
                Order = index,
                VertexId = vertexId
            };
            range.Insert(index, pathfindingRange);
            for (int i = 0; i < range.Count; i++)
            {
                range[i].IsSource = i == 0;
                range[i].IsTarget = i == range.Count - 1 && range.Count > 1;
                range[i].Order = i;
            }
            await unit.RangeRepository.UpsertAsync(range, t).ConfigureAwait(false);
            return true;
        }, token).ConfigureAwait(false);
    }

    public async Task<RunStatisticsModel> ReadStatisticAsync(int runId, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var statistic = await unit.StatisticsRepository
                .ReadByIdAsync(runId, t).ConfigureAwait(false);
            return statistic.ToRunStatisticsModel();
        }, token).ConfigureAwait(false);
    }

    public async Task<GraphInformationModel> ReadGraphInfoAsync(int graphId, CancellationToken token = default)
    {
        await using var unitOfWork = factory.Create();
        var result = await unitOfWork.GraphRepository.ReadAsync(graphId, token)
            .ConfigureAwait(false);
        return result.ToGraphInformationModel();
    }

    public async Task<bool> UpdateGraphInfoAsync(GraphInformationModel graph, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var graphInfo = graph.ToGraphEntity();
            return await unit.GraphRepository.UpdateAsync(graphInfo, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    private static async Task<GraphModel<T>> ReadGraphInternalAsync(IUnitOfWork unit, int graphId,
        CancellationToken token = default)
    {
        var graphEntity = await unit.GraphRepository
            .ReadAsync(graphId, token).ConfigureAwait(false);
        var vertices = await unit.VerticesRepository
            .ReadVerticesByGraphIdAsync(graphId)
            .ToVerticesAsync<T>(token)
            .ConfigureAwait(false);
        return new()
        {
            Vertices = vertices,
            DimensionSizes = graphEntity.Dimensions.ToDimensionSizes(),
            Id = graphEntity.Id,
            Name = graphEntity.Name,
            Neighborhood = graphEntity.Neighborhood,
            SmoothLevel = graphEntity.SmoothLevel,
            Status = graphEntity.Status
        };
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(IEnumerable<int> runIds, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByIdsAsync([.. runIds])
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> CreateStatisticsAsync(IEnumerable<CreateStatisticsRequest> request, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var entities = request.Select(x => x.ToStatistics()).ToArray();
            var result = await unit.StatisticsRepository.CreateAsync(entities, t).ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    private static async Task<GraphModel<T>> CreateGraphAsyncInternal(IUnitOfWork unit,
        CreateGraphRequest<T> request, CancellationToken token = default)
    {
        var graph = request.ToGraphEntity();
        await unit.GraphRepository.CreateAsync(graph, token).ConfigureAwait(false);
        var vertices = request.Graph.ToVertexEntities();
        vertices.ForEach(x => x.GraphId = graph.Id);
        await unit.VerticesRepository.CreateAsync(vertices, token).ConfigureAwait(false);
        vertices.Zip(request.Graph, (x, y) => (Entity: x, Vertex: y))
            .ForEach(x => x.Vertex.Id = x.Entity.Id);
        return new()
        {
            DimensionSizes = request.Graph.DimensionsSizes,
            Vertices = request.Graph,
            Id = graph.Id,
            Name = graph.Name,
            Neighborhood = graph.Neighborhood,
            SmoothLevel = graph.SmoothLevel,
            Status = graph.Status
        };
    }

    private static async Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeAsyncInternal(
        IUnitOfWork unit, int graphId, CancellationToken token = default)
    {
        var range = await unit.RangeRepository
            .ReadByGraphIdAsync(graphId)
            .ToListAsync(token)
            .ConfigureAwait(false);
        var result = new List<PathfindingRangeModel>(range.Count);
        foreach (var rangeVertex in range)
        {
            var vertex = await unit.VerticesRepository
                .ReadAsync(rangeVertex.VertexId, token)
                .ConfigureAwait(false);
            var model = rangeVertex.ToRangeModel();
            model.Position = vertex.Coordinates.ToCoordinates();
            result.Add(model);
        }
        return result.AsReadOnly();
    }
}
