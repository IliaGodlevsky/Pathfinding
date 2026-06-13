using Pathfinding.Data.InMemory;
using Pathfinding.Domain;
using Pathfinding.Domain.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;

namespace Pathfinding.Service.Services;

public sealed class RangeRequestService<T>(IUnitOfWorkFactory factory) : IRangeRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
    public RangeRequestService() : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeOrderedAsync(int graphId,
        CancellationToken token = default)
    {
        return ExecuteAsync<IReadOnlyCollection<PathfindingRangeModel>>(async (unit, t) =>
        {
            var range = await unit.RangeRepository
                .ReadByGraphIdOrderedByOrderAsync(graphId)
                .Select(x => x.ToRangeModel())
                .ToListAsync(t)
                .ConfigureAwait(false);
            return range;
        }, token);
    }

    public Task<bool> CreatePathfindingVertexAsync(CreatePathfindingVertexRequest request,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unit, t) =>
        {
            var range = await unit.RangeRepository
                .ReadByGraphIdOrderedByOrderAsync(request.GraphId)
                .ToListAsync(t)
                .ConfigureAwait(false);

            range.Insert(request.Index, request.ToPathfindingRange());

            for (var i = 0; i < range.Count; i++)
            {
                range[i].Order = i;
            }

            await unit.RangeRepository
                .UpsertAsync(range, t)
                .ConfigureAwait(false);
            return true;
        }, token);
    }

    public Task<bool> DeleteRangeAsync(IEnumerable<T> request,
        CancellationToken token = default)
    {
        return ExecuteAsync((unit, t) =>
        {
            var verticesIds = request.Select(x => x.Id).ToList();
            return unit.RangeRepository.DeleteByVerticesIdsAsync(verticesIds, t);
        }, token);
    }

    public Task<bool> DeleteRangeAsync(int graphId,
        CancellationToken token = default)
    {
        return ExecuteAsync(
            (unit, t) => unit.RangeRepository.DeleteByGraphIdAsync(graphId, t),
            token);
    }

    private async Task<TResult> ExecuteAsync<TResult>(
        Func<IUnitOfWork, CancellationToken, Task<TResult>> action,
        CancellationToken token)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        return await action(unit, token).ConfigureAwait(false);
    }
}
