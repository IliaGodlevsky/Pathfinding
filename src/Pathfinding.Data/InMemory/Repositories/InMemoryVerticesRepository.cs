using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Data.InMemory.Repositories;

internal sealed class InMemoryVerticesRepository 
    : InMemoryRepository<long, Vertex>, IVerticesRepository
{
    public Task<bool> DeleteVerticesByGraphIdAsync(int graphId)
    {
        var result = Set.RemoveWhere(x => x.GraphId == graphId);
        return Task.FromResult(result > 0);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
    {
        return Set.Where(x => x.GraphId == graphId).ToAsyncEnumerable();
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
            if (Set.TryGetValue(vertex, out var result))
            {
                Set.Remove(result);
                Set.Add(vertex);
            }
        }
        return Task.FromResult(true);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByIdsAsync(IReadOnlyCollection<long> vertexIds)
    {
        return Set.Where(x => vertexIds.Contains(x.Id)).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdsAsync(IReadOnlyCollection<int> graphIds)
    {
        return Set.Where(x => graphIds.Contains(x.GraphId)).ToAsyncEnumerable();
    }

    protected override long NextId()
    {
        return id++;
    }
}