using Pathfinding.Domain;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.Service.Interface;

public interface IDataTransferRequestService<T>
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

    ValueTask<IReadOnlyCollection<PathfindingHistoryModel<T>>> CreatePathfindingHistoriesAsync(
        IReadOnlyCollection<PathfindingHistorySerializationModel> request,
        CancellationToken token = default);
}
