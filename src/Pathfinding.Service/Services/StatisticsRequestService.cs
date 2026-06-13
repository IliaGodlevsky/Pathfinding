using Pathfinding.Data.InMemory;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Read;

namespace Pathfinding.Service.Services;

public sealed class StatisticsRequestService(IUnitOfWorkFactory factory) : IStatisticsRequestService
{
    public StatisticsRequestService() : this(new InMemoryUnitOfWorkFactory())
    {
    }

    public Task<bool> DeleteRunsAsync(
        IReadOnlyCollection<int> runIds,
        CancellationToken token = default)
    {
        return ExecuteAsync(
            (unit, t) => unit.StatisticsRepository.DeleteByIdsAsync(runIds, t),
            token);
    }

    public Task<RunStatisticsModel> ReadStatisticAsync(
        int runId,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unit, t) =>
        {
            var statistic = await unit.StatisticsRepository
                .ReadByIdAsync(runId, t)
                .ConfigureAwait(false);
            return statistic.ToRunStatisticsModel();
        }, token);
    }

    public Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(
        IReadOnlyCollection<int> runIds,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByIdsAsync(runIds)
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token);
    }

    public Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(
        ReadStatisticsRequest request,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByGraphIdAsync(request.GraphId, request.Skip, request.Take)
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token);
    }

    public Task<IReadOnlyCollection<RunStatisticsModel>> CreateStatisticsAsync(
        IReadOnlyCollection<CreateStatisticsRequest> request,
        CancellationToken token = default)
    {
        return ExecuteAsync(async (unit, t) =>
        {
            var entities = request
                .Select(x => x.ToStatistics())
                .ToArray();
            var result = await unit.StatisticsRepository
                .CreateAsync(entities, t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token);
    }

    public Task<bool> UpdateStatisticsAsync(
        IReadOnlyCollection<RunStatisticsModel> models,
        CancellationToken token = default)
    {
        return ExecuteAsync(
            (unit, t) => unit.StatisticsRepository.UpdateAsync(models.ToStatistics(), t),
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
