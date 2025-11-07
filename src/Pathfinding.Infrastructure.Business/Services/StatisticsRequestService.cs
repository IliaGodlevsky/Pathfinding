using Pathfinding.Domain.Interface.Extensions;
using Pathfinding.Domain.Interface.Factories;
using Pathfinding.Infrastructure.Data.InMemory;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;

namespace Pathfinding.Infrastructure.Business.Services;

public sealed class StatisticsRequestService(IUnitOfWorkFactory factory) : IStatisticsRequestService
{
    public StatisticsRequestService() : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public async Task<bool> DeleteRunsAsync(IEnumerable<int> runIds, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) => await unit.StatisticsRepository
                .DeleteByIdsAsync([.. runIds], t)
                .ConfigureAwait(false), token).ConfigureAwait(false);
    }

    public async Task<RunStatisticsModel> ReadStatisticAsync(int runId, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var statistic = await unit.StatisticsRepository
                .ReadByIdAsync(runId, t).ConfigureAwait(false);
            return statistic.ToRunStatisticsModel();
        }, token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(IEnumerable<int> runIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByIdsAsync([.. runIds])
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(int graphId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByGraphIdAsync(graphId)
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> CreateStatisticsAsync(
        IEnumerable<CreateStatisticsRequest> request, CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var entities = request.Select(x => x.ToStatistics()).ToArray();
            var result = await unit.StatisticsRepository.CreateAsync(entities, t).ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> UpdateStatisticsAsync(IEnumerable<RunStatisticsModel> models,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var entities = models.ToStatistics();
            return await unit.StatisticsRepository.UpdateAsync(entities, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }
}
