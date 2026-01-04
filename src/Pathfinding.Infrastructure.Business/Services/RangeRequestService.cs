using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Business.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;

namespace Pathfinding.Infrastructure.Business.Services;

public sealed class RangeRequestService<T>(IUnitOfWorkFactory factory) : IRangeRequestService<T>
    where T : IVertex, IEntity<long>, new()
{
    public async Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            return await unit.RangeRepository
                .ReadByGraphIdAsync(graphId)
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
                .ReadByGraphIdAsync(request.GraphId)
                .ToListAsync(t)
                .ConfigureAwait(false);

            range.Insert(request.Index, request.ToPathfindingRange());

            for (int i = 0; i < range.Count; i++)
            {
                range[i].IsSource = i == 0;
                range[i].IsTarget = i == range.Count - 1 && range.Count > 1;
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
