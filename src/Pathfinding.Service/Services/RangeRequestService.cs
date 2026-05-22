using Pathfinding.Data.InMemory;
using Pathfinding.Domain;
using Pathfinding.Domain.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
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

    public async Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeOrderedAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            return await unit.RangeRepository
                .ReadByGraphIdOrderedByOrderAsync(graphId)
                .Select(x => x.ToRangeModel())
                .ToListAsync(token)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> CreatePathfindingVertexAsync(CreatePathfindingVertexRequest request,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var range = await unit.RangeRepository
                .ReadByGraphIdOrderedByOrderAsync(request.GraphId)
                .ToListAsync(t)
                .ConfigureAwait(false);

            range.Insert(request.Index, request.ToPathfindingRange());

            for (int i = 0; i < range.Count; i++)
            {
                range[i].Order = i;
            }

            await unit.RangeRepository
                .UpsertAsync(range, t)
                .ConfigureAwait(false);
            return true;
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> DeleteRangeAsync(IEnumerable<T> request,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            var verticesIds = request.Select(x => x.Id).ToList();
            return await unitOfWork.RangeRepository
                .DeleteByVerticesIdsAsync(verticesIds, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> DeleteRangeAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unitOfWork, t) =>
        {
            return await unitOfWork.RangeRepository
                .DeleteByGraphIdAsync(graphId, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }
}
