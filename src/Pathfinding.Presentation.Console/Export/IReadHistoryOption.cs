using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.Presentation.Console.Export;

internal interface IReadHistoryOption
{
    Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);
}