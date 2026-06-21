using Pathfinding.Domain;
using Pathfinding.Domain.Interface;
using Pathfinding.Serialization.Models;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.Serialization.Services;

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
