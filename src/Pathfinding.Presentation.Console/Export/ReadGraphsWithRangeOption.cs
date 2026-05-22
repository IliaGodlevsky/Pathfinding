using Pathfinding.Presentation.Console.Models;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.Presentation.Console.Export;

internal sealed class ReadGraphsWithRangeOption(IGraphRequestService<GraphVertexModel> service) : IReadHistoryOption
{
    public async Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        return await service.ReadSerializationGraphsWithRangeAsync(graphIds, token).ConfigureAwait(false);
    }
}