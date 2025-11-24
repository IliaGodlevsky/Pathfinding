using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Business.Extensions;

internal static class UnitOfWorkExtensions
{
    internal static async Task<IReadOnlyCollection<GraphModel<T>>> ReadGraphsInternalAsync<T>(
        this IUnitOfWork unit, IReadOnlyCollection<int> graphIds, CancellationToken token = default)
        where T : IVertex, IEntity<long>, new()
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
                DimensionSizes = graph.Dimensions.ToDimensionSizes(),
                Id = graph.Id,
                Name = graph.Name,
                Neighborhood = graph.Neighborhood,
                SmoothLevel = graph.SmoothLevel,
                Status = graph.Status,
                Vertices = vertices[graph.Id]
            });
        }
        return models;
    }

    internal static async Task<GraphModel<T>> CreateGraphAsyncInternal<T>(
        this IUnitOfWork unit, CreateGraphRequest<T> request, CancellationToken token = default)
        where T : IVertex, IEntity<long>, new()
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

    internal static async Task<IReadOnlyDictionary<int, IReadOnlyCollection<PathfindingRangeModel>>> ReadRangesAsyncInternal(
        this IUnitOfWork unit, IReadOnlyCollection<int> graphIds, CancellationToken token = default)
    {
        var ranges = (await unit.RangeRepository
            .ReadByGraphIdsAsync(graphIds)
            .ToArrayAsync(token))
            .GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.OrderBy(y => y.Order).ToArray());
        var result = new Dictionary<int, IReadOnlyCollection<PathfindingRangeModel>>();
        foreach (var range in ranges)
        {
            result.Add(range.Key, [.. range.Value.Select(x => x.ToRangeModel())]);
        }
        return result;
    }
}
