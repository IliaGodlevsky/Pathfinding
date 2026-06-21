using Pathfinding.Presentation.Console.Models;
using Pathfinding.Serialization.Models;
using Pathfinding.Serialization.Services;

namespace Pathfinding.Presentation.Console.Export;

internal sealed class ReadGraphWithRunsOptions(IDataTransferRequestService<GraphVertexModel> service) : IReadHistoryOption
{
    public Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return service.ReadSerializationHistoriesAsync(graphIds, token);
    }
}