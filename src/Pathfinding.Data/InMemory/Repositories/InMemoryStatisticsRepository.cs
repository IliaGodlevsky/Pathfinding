using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Data.InMemory.Repositories;

internal sealed class InMemoryStatisticsRepository 
    : InMemoryRepository<int, Statistics>, IStatisticsRepository
{
    public IAsyncEnumerable<Statistics> ReadByGraphIdAsync(int graphId, int skip, int take)
    {
        return Set
            .Where(s => s.GraphId == graphId)
            .Skip(skip)
            .Take(take)
            .ToAsyncEnumerable();
    }

    public Task<bool> DeleteByGraphId(int graphId)
    {
        bool removed = Set.RemoveWhere(s => s.GraphId == graphId) > 0;
        return Task.FromResult(removed);
    }

    public Task<bool> DeleteByIdsAsync(IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var removed = Set.RemoveWhere(s => ids.Contains(s.Id)) > 0;
        return Task.FromResult(removed);
    }

    public Task<Statistics> ReadByIdAsync(int statId, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<Statistics>(token);
        }
        var tracking = new Statistics { Id = statId };
        Set.TryGetValue(tracking, out var statistics);
        return Task.FromResult(statistics);
    }

    public Task<bool> UpdateAsync(
        IReadOnlyCollection<Statistics> entities,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        foreach (var entity in entities)
        {
            if (Set.TryGetValue(entity, out var statistics))
            {
                statistics.StepRule = entity.StepRule;
                statistics.Steps = entity.Steps;
                statistics.Heuristics = entity.Heuristics;
                statistics.ResultStatus = entity.ResultStatus;
                statistics.Cost = entity.Cost;
                statistics.Algorithm = entity.Algorithm;
                statistics.Weight = entity.Weight;
                statistics.Visited = entity.Visited;
                statistics.Elapsed = entity.Elapsed;
            }
        }
        return Task.FromResult(true);
    }

    public IAsyncEnumerable<Statistics> ReadByIdsAsync(
        IReadOnlyCollection<int> runIds)
    {
        return Set.Where(x => runIds.Contains(x.Id)).ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Statistics> ReadByGraphIdsAsync(IReadOnlyCollection<int> graphIds)
    {
        return Set.Where(x => graphIds.Contains(x.GraphId)).ToAsyncEnumerable();
    }

    protected override int NextId()
    {
        return id++;
    }
}