using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories;

internal sealed class InMemoryVerticesRepository : IVerticesRepository
{
    private long id;

    private readonly HashSet<Vertex> set = new(EntityComparer<long>.Instance);

    public Task<IReadOnlyCollection<Vertex>> CreateAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IReadOnlyCollection<Vertex>>(token);
        }
        foreach (var vertex in vertices)
        {
            vertex.Id = ++id;
            set.Add(vertex);
        }
        return Task.FromResult(vertices);
    }

    public Task<bool> DeleteVerticesByGraphIdAsync(int graphId)
    {
        var result = set.RemoveWhere(x => x.GraphId == graphId);
        return Task.FromResult(result > 0);
    }

    public Task<Vertex> ReadAsync(long vertexId,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<Vertex>(token);
        }
        var vertex = new Vertex { Id = vertexId };
        set.TryGetValue(vertex, out var result);
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
    {
        return set.Where(x => x.GraphId == graphId).ToAsyncEnumerable();
    }

    public Task<bool> UpdateVerticesAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        foreach (var vertex in vertices)
        {
            if (set.TryGetValue(vertex, out var result))
            {
                set.Remove(result);
                set.Add(vertex);
            }
        }
        return Task.FromResult(true);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByIdsAsync(IReadOnlyCollection<long> vertexIds)
    {
        return set.Where(x => vertexIds.Contains(x.Id)).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdsAsync(IReadOnlyCollection<int> graphIds)
    {
        return set.Where(x => graphIds.Contains(x.GraphId)).ToAsyncEnumerable();
    }
}