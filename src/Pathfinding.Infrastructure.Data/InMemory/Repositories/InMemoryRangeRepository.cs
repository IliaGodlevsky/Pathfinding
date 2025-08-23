using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories;

internal sealed class InMemoryRangeRepository : IRangeRepository
{
    private int id;

    private readonly HashSet<PathfindingRange> set = new(EntityComparer<int>.Instance);

    public Task<IReadOnlyCollection<PathfindingRange>> CreateAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default)
    {
        var result = entities
            .ForEach(x => x.Id = ++id)
            .ForWhole(set.AddRange)
            .ToList();
        return Task.FromResult((IReadOnlyCollection<PathfindingRange>)result);
    }

    public Task<bool> DeleteByGraphIdAsync(int graphId,
        CancellationToken token = default)
    {
        var result = set.RemoveWhere(x => x.GraphId == graphId);
        return Task.FromResult(result > 0);
    }

    public Task<bool> DeleteByVerticesIdsAsync(IReadOnlyCollection<long> verticesIds,
        CancellationToken token = default)
    {
        var result = set.RemoveWhere(x => verticesIds.Contains(x.VertexId));
        return Task.FromResult(result > 0);
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdAsync(int graphId)
    {
        return set.Where(x => x.GraphId == graphId)
            .OrderBy(x => x.Order)
            .ToAsyncEnumerable();
    }

    public Task<IReadOnlyCollection<PathfindingRange>> UpsertAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default)
    {
        foreach (var entity in entities)
        {
            if (set.TryGetValue(entity, out var value))
            {
                set.Remove(value);
                set.Add(entity);
            }
            else
            {
                entity.Id = ++id;
                set.Add(entity);
            }
        }
        return Task.FromResult(entities);
    }
}