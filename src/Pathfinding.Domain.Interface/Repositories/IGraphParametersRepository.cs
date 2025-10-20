using Pathfinding.Domain.Core.Entities;

namespace Pathfinding.Domain.Interface.Repositories;

public interface IGraphParametersRepository
{
    Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);

    Task<Graph> ReadAsync(int graphId, CancellationToken token = default);

    Task<Graph> CreateAsync(Graph graph, CancellationToken token = default);

    Task<bool> DeleteAsync(IReadOnlyCollection<int> graphIds,
        CancellationToken token = default);

    Task<bool> UpdateAsync(Graph graph, CancellationToken token = default);

    IAsyncEnumerable<Graph> GetAll();
}
