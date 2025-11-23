using Pathfinding.Domain.Core.Entities;

namespace Pathfinding.Domain.Interface.Repositories;

public interface IRangeRepository
{
    Task<IReadOnlyCollection<PathfindingRange>> CreateAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default);

    Task<IReadOnlyCollection<PathfindingRange>> UpsertAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default);

    IAsyncEnumerable<PathfindingRange> ReadByGraphIdAsync(int graphId);

    IAsyncEnumerable<PathfindingRange> ReadByGraphIdsAsync(IReadOnlyCollection<int> ids);

    Task<bool> DeleteByVerticesIdsAsync(
        IReadOnlyCollection<long> verticesIds,
        CancellationToken token = default);

    Task<bool> DeleteByGraphIdAsync(int graphId,
        CancellationToken token = default);
}
