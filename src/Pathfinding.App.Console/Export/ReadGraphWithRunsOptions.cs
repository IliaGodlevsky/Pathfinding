﻿using Pathfinding.App.Console.Models;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models.Serialization;

namespace Pathfinding.App.Console.Export;

internal sealed class ReadGraphWithRunsOptions(IRequestService<GraphVertexModel> service) : IReadHistoryOption
{
    public async Task<PathfindingHistoriesSerializationModel> ReadHistoryAsync(IReadOnlyCollection<int> graphIds)
    {
        return await service.ReadSerializationHistoriesAsync(graphIds).ConfigureAwait(false);
    }
}