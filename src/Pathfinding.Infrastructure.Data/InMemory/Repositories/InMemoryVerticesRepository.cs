using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories;

internal sealed class InMemoryVerticesRepository : IVerticesRepository
{
    private long id;

    private readonly HashSet<Vertex> set = new(EntityComparer<long>.Instance);

    public Task<IReadOnlyCollection<Vertex>> CreateAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        var result = vertices
            .ForEach(x => x.Id = ++id)
            .ForWhole(set.AddRange)
            .ToList();
        return Task.FromResult((IReadOnlyCollection<Vertex>)result);
    }

    public Task<bool> DeleteVerticesByGraphIdAsync(int graphId)
    {
        var result = set.RemoveWhere(x => x.GraphId == graphId);
        return Task.FromResult(result > 0);
    }

    public Task<Vertex> ReadAsync(long vertexId,
        CancellationToken token = default)
    {
        var vertex = new Vertex { Id = vertexId };
        set.TryGetValue(vertex, out var result);
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
    {
        return set
            .Where(x => x.GraphId == graphId)
            .ToAsyncEnumerable();
    }

    public Task<bool> UpdateVerticesAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
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
}