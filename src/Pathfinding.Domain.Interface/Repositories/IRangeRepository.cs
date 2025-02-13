using Pathfinding.Domain.Core.Entities;

namespace Pathfinding.Domain.Interface.Repositories
{
    public interface IRangeRepository
    {
        Task<IEnumerable<PathfindingRange>> CreateAsync(IEnumerable<PathfindingRange> entities,
            CancellationToken token = default);

        Task<IEnumerable<PathfindingRange>> UpsertAsync(IEnumerable<PathfindingRange> entities,
            CancellationToken token = default);

        Task<IEnumerable<PathfindingRange>> ReadByGraphIdAsync(int graphId,
            CancellationToken token = default);

        Task<bool> DeleteByVerticesIdsAsync(IEnumerable<long> verticesIds,
            CancellationToken token = default);

        Task<bool> DeleteByGraphIdAsync(int graphId, CancellationToken token = default);
    }
}
