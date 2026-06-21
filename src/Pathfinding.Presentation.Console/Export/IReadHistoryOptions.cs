using Pathfinding.Presentation.Console.Models;
using Pathfinding.Serialization.Models;

namespace Pathfinding.Presentation.Console.Export;

internal interface IReadHistoryOptions
{
    IReadOnlyList<ExportOptions> AvailableExportOptions { get; }

    Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        ExportOptions option,
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);
}