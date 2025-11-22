using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;

namespace Pathfinding.Service.Interface;

public interface IGraphRequestService<T>
    where T : IVertex, IEntity<long>
{
    Task<PathfindingHistoriesSerializationModel> ReadSerializationHistoriesAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);

    Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);

    Task<PathfindingHistoriesSerializationModel> ReadSerializationGraphsWithRangeAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);

    Task<GraphModel<T>> ReadGraphAsync(int graphId, CancellationToken token = default);

    Task<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(
        IReadOnlyCollection<PathfindingHistorySerializationModel> request,
        CancellationToken token = default);

    Task<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default);

    Task<bool> UpdateVerticesAsync(UpdateVerticesRequest<T> request,
        CancellationToken token = default);
}
