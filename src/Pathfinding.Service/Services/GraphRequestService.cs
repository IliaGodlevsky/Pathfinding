using Pathfinding.Data.InMemory;
using Pathfinding.Domain;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Service.Services;

public sealed class GraphRequestService<T>(IUnitOfWorkFactory factory) : IGraphRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
    public GraphRequestService() : this(new InMemoryUnitOfWorkFactory())
    {

    }

    public async ValueTask<bool> UpdateVerticesAsync(
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

    public async ValueTask<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(
            (unit, t) => unit.CreateGraphAsyncInternal(graph, t),
            token).ConfigureAwait(false);
    }
}
