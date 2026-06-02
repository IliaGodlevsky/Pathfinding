using Pathfinding.Data.InMemory;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Extensions;
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

    public async Task<bool> DeleteRunsAsync(
        IReadOnlyCollection<int> runIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            return await unit.StatisticsRepository
                .DeleteByIdsAsync(runIds, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }

    public async Task<RunStatisticsModel> ReadStatisticAsync(
        int runId,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var statistic = await unit.StatisticsRepository
                .ReadByIdAsync(runId, t)
                .ConfigureAwait(false);
            return statistic.ToRunStatisticsModel();
        }, token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(
        IReadOnlyCollection<int> runIds,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var result = await unit.StatisticsRepository
                .ReadByIdsAsync(runIds)
                .ToListAsync(t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(
        ReadStatisticsRequest request,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token);
        var result = await unitOfWork.StatisticsRepository
            .ReadByGraphIdAsync(request.GraphId, request.Skip, request.Take)
            .ToListAsync(token)
            .ConfigureAwait(false);
        return result.ToRunStatisticsModels();
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> CreateStatisticsAsync(
        IReadOnlyCollection<CreateStatisticsRequest> request,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var entities = request
                .Select(x => x.ToStatistics())
                .ToArray();
            var result = await unit.StatisticsRepository
                .CreateAsync(entities, t)
                .ConfigureAwait(false);
            return result.ToRunStatisticsModels();
        }, token).ConfigureAwait(false);
    }

    public async Task<bool> UpdateStatisticsAsync(
        IReadOnlyCollection<RunStatisticsModel> models,
        CancellationToken token = default)
    {
        return await factory.TransactionAsync(async (unit, t) =>
        {
            var entities = models.ToStatistics();
            return await unit.StatisticsRepository
                .UpdateAsync(entities, t)
                .ConfigureAwait(false);
        }, token).ConfigureAwait(false);
    }
}
