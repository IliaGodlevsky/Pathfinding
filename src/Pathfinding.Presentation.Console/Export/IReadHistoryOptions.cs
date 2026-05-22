using Pathfinding.Presentation.Console.Models;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.Presentation.Console.Export;

internal interface IReadHistoryOptions
{
    IReadOnlyList<ExportOptions> AvailableExportOptions { get; }

    Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        ExportOptions option,
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);
}