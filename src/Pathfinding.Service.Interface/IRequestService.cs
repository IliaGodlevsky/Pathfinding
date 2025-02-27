using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Models.Undefined;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;

namespace Pathfinding.Service.Interface;

public interface IRequestService<T>
    where T : IVertex, IEntity<long>
{
    #region Vertices
    Task<PathfindingHisotiriesSerializationModel> ReadSerializationHistoriesAsync(IEnumerable<int> graphIds,
        CancellationToken token = default);

    Task<PathfindingHisotiriesSerializationModel> ReadSerializationGraphsAsync(IEnumerable<int> graphIds,
        CancellationToken token = default);

    Task<PathfindingHisotiriesSerializationModel> ReadSerializationGraphsWithRangeAsync(IEnumerable<int> graphIds,
        CancellationToken token = default);

    Task<GraphModel<T>> ReadGraphAsync(int graphId, CancellationToken token = default);

    Task<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(IEnumerable<PathfindingHistorySerializationModel> request,
        CancellationToken token = default);

    Task<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default);

    Task<bool> UpdateVerticesAsync(UpdateVerticesRequest<T> request,
        CancellationToken token = default);
    #endregion

    #region Statistics
    Task<bool> DeleteRunsAsync(IEnumerable<int> runIds, CancellationToken token = default);

    Task<RunStatisticsModel> ReadStatisticAsync(int runId, CancellationToken token = default);

    Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(IEnumerable<int> runIds,
        CancellationToken token = default);

    Task<IReadOnlyCollection<RunStatisticsModel>> ReadStatisticsAsync(int graphId,
        CancellationToken token = default);

    Task<IReadOnlyCollection<RunStatisticsModel>> CreateStatisticsAsync(IEnumerable<CreateStatisticsRequest> request,
        CancellationToken token = default);

    Task<bool> UpdateStatisticsAsync(IEnumerable<RunStatisticsModel> models,
        CancellationToken token = default);
    #endregion

    #region GraphInfo
    Task<GraphInformationModel> ReadGraphInfoAsync(int graphId, CancellationToken token = default);

    Task<bool> UpdateGraphInfoAsync(GraphInformationModel graph, CancellationToken token = default);

    Task<IReadOnlyCollection<GraphInformationModel>> ReadAllGraphInfoAsync(CancellationToken token = default);

    Task<bool> DeleteGraphsAsync(IEnumerable<int> ids, CancellationToken token = default);
    #endregion

    #region Range
    Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeAsync(int graphId,
        CancellationToken token = default);

    Task<bool> CreatePathfindingVertexAsync(int graphId, long vertexId,
        int index, CancellationToken token = default);

    Task<bool> DeleteRangeAsync(IEnumerable<T> request, CancellationToken token = default);

    Task<bool> DeleteRangeAsync(int graphId, CancellationToken token = default);
    #endregion
}
