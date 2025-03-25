using Pathfinding.Domain.Core.Entities;

namespace Pathfinding.Domain.Interface.Repositories;

public interface IStatisticsRepository
{
    Task<IEnumerable<Statistics>> ReadByGraphIdAsync(int graphId,
        CancellationToken token = default);

    Task<Statistics> ReadByIdAsync(int runId, CancellationToken token = default);

    Task<IEnumerable<Statistics>> ReadByIdsAsync(IEnumerable<int> runIds,
        CancellationToken token = default);

    Task<int> ReadStatisticsCountAsync(int graphId, CancellationToken token = default);

    Task<Statistics> CreateAsync(Statistics entity, CancellationToken token = default);

    Task<IEnumerable<Statistics>> CreateAsync(IEnumerable<Statistics> statistics, CancellationToken token = default);

    Task<bool> DeleteByGraphId(int graphId, CancellationToken token = default);

    Task<bool> DeleteByIdsAsync(IEnumerable<int> ids, CancellationToken token = default);

    Task<bool> UpdateAsync(IEnumerable<Statistics> entities, CancellationToken token = default);
}
