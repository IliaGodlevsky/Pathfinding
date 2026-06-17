using Pathfinding.Presentation.Console.Models;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.Presentation.Console.Export;

internal sealed class ReadGraphOnlyOption(IDataTransferRequestService<GraphVertexModel> service) : IReadHistoryOption
{
    public Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return service.ReadSerializationGraphsAsync(graphIds, token);
    }
}