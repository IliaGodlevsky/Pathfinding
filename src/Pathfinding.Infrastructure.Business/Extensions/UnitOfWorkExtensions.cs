using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Business.Extensions;

internal static class UnitOfWorkExtensions
{
    internal static async Task<GraphModel<T>> ReadGraphInternalAsync<T>(
        this IUnitOfWork unit, int graphId, CancellationToken token = default)
        where T : IVertex, IEntity<long>, new()
    {
        var graphEntity = await unit.GraphRepository
            .ReadAsync(graphId, token).ConfigureAwait(false);
        var vertices = await unit.VerticesRepository
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
        return new GraphModel<T>
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

    internal static async Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeAsyncInternal<T>(
        this IUnitOfWork unit, int graphId, CancellationToken token = default)
        where T : IVertex, IEntity<long>
    {
        var range = await unit.RangeRepository
            .ReadByGraphIdAsync(graphId)
            .ToListAsync(token)
            .ConfigureAwait(false);
        var rangeVerticesIds = range.Select(x => x.VertexId).ToHashSet();
        var vertices = await unit.VerticesRepository
            .ReadVerticesByIdsAsync(rangeVerticesIds)
            .ToDictionaryAsync(x => x.Id, x => x.Coordinates.ToCoordinates(), token)
            .ConfigureAwait(false);
        var result = new List<PathfindingRangeModel>(range.Count);
        foreach (var rangeVertex in range)
        {
            var model = rangeVertex.ToRangeModel();
            model.Position = vertices[rangeVertex.VertexId];
            result.Add(model);
        }
        return result.AsReadOnly();
    }
}
