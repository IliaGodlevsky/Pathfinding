using Pathfinding.Domain.Core.Entities;

namespace Pathfinding.Domain.Interface.Repositories;

public interface IStatisticsRepository
{
    IAsyncEnumerable<Statistics> ReadByGraphIdAsync(int graphId);

    Task<Statistics> ReadByIdAsync(
        int runId, CancellationToken token = default);

    IAsyncEnumerable<Statistics> ReadByIdsAsync(IReadOnlyCollection<int> runIds);

    Task<IReadOnlyCollection<Statistics>> CreateAsync(
        IReadOnlyCollection<Statistics> statistics, 
        CancellationToken token = default);

    Task<bool> DeleteByIdsAsync(
        IReadOnlyCollection<int> ids, CancellationToken token = default);

    Task<bool> UpdateAsync(
        IReadOnlyCollection<Statistics> entities, 
        CancellationToken token = default);
}
