using Pathfinding.Domain.Core;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.Service.Interface;

public interface IRangeRequestService<T>
    where T : IVertex, IEntity<long>
{
    Task<IReadOnlyCollection<PathfindingRangeModel>> ReadRangeAsync(int graphId,
        CancellationToken token = default);

    Task<bool> CreatePathfindingVertexAsync(int graphId,
        long vertexId, int index, CancellationToken token = default);

    Task<bool> DeleteRangeAsync(IEnumerable<T> request, CancellationToken token = default);

    Task<bool> DeleteRangeAsync(int graphId, CancellationToken token = default);
}
