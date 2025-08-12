using Pathfinding.App.Console.Models;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.App.Console.Export;

internal interface IReadHistoryOptionsFacade
{
    IReadOnlyList<ExportOptions> Allowed { get; }

    Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        ExportOptions option,
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);
}