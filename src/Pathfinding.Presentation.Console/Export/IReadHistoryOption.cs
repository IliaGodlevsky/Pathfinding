using Pathfinding.Serialization.Models;

namespace Pathfinding.Presentation.Console.Export;

internal interface IReadHistoryOption
{
    Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);
}