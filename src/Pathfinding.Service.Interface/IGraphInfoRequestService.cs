using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.Service.Interface;

public interface IGraphInfoRequestService
{
    Task<GraphInformationModel> ReadGraphInfoAsync(int graphId, CancellationToken token = default);

    Task<bool> UpdateGraphInfoAsync(GraphInformationModel graph, CancellationToken token = default);

    Task<IReadOnlyCollection<GraphInformationModel>> ReadAllGraphInfoAsync(CancellationToken token = default);

    Task<bool> DeleteGraphsAsync(IEnumerable<int> ids, CancellationToken token = default);
}
