using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Data.InMemory.Repositories;

internal sealed class InMemoryRangeRepository 
    : InMemoryRepository<int, PathfindingRange>, IRangeRepository
{
    public Task<bool> DeleteByGraphIdAsync(int graphId,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var result = Set.RemoveWhere(x => x.GraphId == graphId);
        return Task.FromResult(result > 0);
    }

    public Task<bool> DeleteByVerticesIdsAsync(IReadOnlyCollection<long> verticesIds,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var result = Set.RemoveWhere(x => verticesIds.Contains(x.VertexId));
        return Task.FromResult(result > 0);
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdOrderedByOrderAsync(int graphId)
    {
        return Set.Where(x => x.GraphId == graphId)
            .OrderBy(x => x.Order)
            .ToAsyncEnumerable();
    }

    public Task<IReadOnlyCollection<PathfindingRange>> UpsertAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IReadOnlyCollection<PathfindingRange>>(token);
        }
        foreach (var entity in entities)
        {
            if (Set.TryGetValue(entity, out var value))
            {
                Set.Remove(value);
                Set.Add(entity);
            }
            else
            {
                entity.Id = Interlocked.Increment(ref id);
                Set.Add(entity);
            }
        }
        return Task.FromResult(entities);
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdsAsync(IReadOnlyCollection<int> ids)
    {
        return Set.Where(x => ids.Contains(x.GraphId)).ToAsyncEnumerable();
    }

    protected override int NextId()
    {
        return id++;
    }
}