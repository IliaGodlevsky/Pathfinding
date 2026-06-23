using Pathfinding.Domain.Interface;
using Pathfinding.Service.Extensions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Read;

namespace Pathfinding.Service.Services;

public sealed class StatisticsRequestService(IUnitOfWorkFactory factory) : IStatisticsRequestService
{
    public async Task<bool> DeleteRunsAsync(
        IReadOnlyCollection<int> runIds,
        CancellationToken token = default)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        return await unit.StatisticsRepository
            .DeleteByIdsAsync(runIds, token)
            .ConfigureAwait(false);
    }

    public async Task<RunStatisticsModel> ReadStatisticAsync(
        int runId,
        CancellationToken token = default)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        var statistic = await unit.StatisticsRepository
            .ReadByIdAsync(runId, token)
            .ConfigureAwait(false);
        return statistic.ToRunStatisticsModel();
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(
        IReadOnlyCollection<int> runIds,
        CancellationToken token = default)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        var result = await unit.StatisticsRepository
            .ReadByIdsAsync(runIds)
            .ToListAsync(token)
            .ConfigureAwait(false);
        return result.ToRunStatisticsModels();
    }

    public async Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(
        ReadStatisticsRequest request,
        CancellationToken token = default)
    {
        await using var unitOfWork = await factory.CreateAsync(token).ConfigureAwait(false);
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
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        var entities = request
            .Select(x => x.ToStatistics())
            .ToArray();
        var result = await unit.StatisticsRepository
            .CreateAsync(entities, token)
            .ConfigureAwait(false);
        return result.ToRunStatisticsModels();
    }

    public async Task<bool> UpdateStatisticsAsync(
        IReadOnlyCollection<RunStatisticsModel> models,
        CancellationToken token = default)
    {
        await using var unit = await factory.CreateAsync(token).ConfigureAwait(false);
        var entities = models.ToStatistics();
        return await unit.StatisticsRepository
            .UpdateAsync(entities, token)
            .ConfigureAwait(false);
    }
}
