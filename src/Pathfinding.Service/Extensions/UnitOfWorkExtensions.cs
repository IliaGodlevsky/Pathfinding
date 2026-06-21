using Pathfinding.Domain;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Service.Extensions;

public static class UnitOfWorkExtensions
{
    public static async Task<GraphModel<T>> CreateGraphAsyncInternal<T>(
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
            Status = graph.Status,
            CostRange = request.Graph.CostRange
        };
    }
}
