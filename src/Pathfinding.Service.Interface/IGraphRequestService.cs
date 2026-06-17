using Pathfinding.Domain;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface.Models.Read;
using Pathfinding.Service.Interface.Requests.Create;
using Pathfinding.Service.Interface.Requests.Update;

namespace Pathfinding.Service.Interface;

public interface IGraphRequestService<T>
    where T : IVertex, IEntity<long>
{
    Task<GraphModel<T>> ReadGraphAsync(int graphId, CancellationToken token = default);

    ValueTask<GraphModel<T>> CreateGraphAsync(CreateGraphRequest<T> graph,
        CancellationToken token = default);

    ValueTask<bool> UpdateVerticesAsync(UpdateVerticesRequest<T> request,
        CancellationToken token = default);
}
