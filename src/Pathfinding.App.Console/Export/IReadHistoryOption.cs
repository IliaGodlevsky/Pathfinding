using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.App.Console.Export;

internal interface IReadHistoryOption
{
    Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(IReadOnlyCollection<int> graphIds);
}