using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Read;

namespace Pathfinding.Service.Interface;

public interface IStatisticsRequestService
{
    Task<bool> DeleteRunsAsync(IReadOnlyCollection<int> runIds, CancellationToken token = default);

    Task<RunStatisticsModel> ReadStatisticAsync(int runId, CancellationToken token = default);

    Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(IReadOnlyCollection<int> runIds,
        CancellationToken token = default);

    Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(ReadStatisticsRequest request,
        CancellationToken token = default);

    Task<IReadOnlyCollection<RunStatisticsModel>> CreateStatisticsAsync(
        IReadOnlyCollection<CreateStatisticsRequest> request,
        CancellationToken token = default);

    Task<bool> UpdateStatisticsAsync(IReadOnlyCollection<RunStatisticsModel> models,
        CancellationToken token = default);
}
