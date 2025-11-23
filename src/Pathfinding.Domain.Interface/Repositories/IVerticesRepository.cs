using Pathfinding.Domain.Core.Entities;

namespace Pathfinding.Domain.Interface.Repositories;

public interface IVerticesRepository
{
    IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId);

    IAsyncEnumerable<Vertex> ReadVerticesByGraphIdsAsync(IReadOnlyCollection<int> graphIds);

    Task<IReadOnlyCollection<Vertex>> CreateAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default);

    Task<bool> UpdateVerticesAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default);

    Task<Vertex> ReadAsync(long vertexId, CancellationToken token = default);

    IAsyncEnumerable<Vertex> ReadVerticesByIdsAsync(IReadOnlyCollection<long> vertexIds);
}
