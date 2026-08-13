using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Data.InMemory.Repositories;

internal sealed class InMemoryGraphParametersRepository(
    InMemoryRangeRepository rangeRepository,
    InMemoryVerticesRepository verticesRepository,
    InMemoryStatisticsRepository statisticsRepository) : InMemoryRepository<int, Graph>, IGraphParametersRepository
{
    public async Task<Graph> CreateAsync(Graph graph,
        CancellationToken token = default)
    {
        var result = await CreateAsync([graph], token).ConfigureAwait(false);
        return result.FirstOrDefault();
    }

    public async Task<bool> DeleteAsync(int graphId,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        // Order sensitive. Do not change the order of deleting
        // Reason: some repositories need the presence of values in the database
        await rangeRepository.DeleteByGraphIdAsync(graphId, token).ConfigureAwait(false);
        await verticesRepository.DeleteVerticesByGraphIdAsync(graphId).ConfigureAwait(false);
        await statisticsRepository.DeleteByGraphId(graphId).ConfigureAwait(false);
        int deleted = Set.RemoveWhere(x => x.Id == graphId);
        return deleted == 1;
    }

    public async Task<bool> DeleteAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        foreach (var graphId in graphIds)
        {
            await DeleteAsync(graphId, token).ConfigureAwait(false);
        }
        return true;
    }

    public IAsyncEnumerable<Graph> GetAll()
    {
        return Set.ToAsyncEnumerable();
    }

    public Task<bool> UpdateAsync(Graph graph,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var equal = new Graph { Id = graph.Id };
        if (Set.TryGetValue(equal, out var result))
        {
            result.Dimensions = graph.Dimensions;
            result.Name = graph.Name;
            result.Neighborhood = graph.Neighborhood;
            result.SmoothLevel = graph.SmoothLevel;
            result.Status = graph.Status;
            result.UpperValueRange = graph.UpperValueRange;
            result.LowerValueRange = graph.LowerValueRange;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public async Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var result = new Dictionary<int, int>();
        foreach (var graphId in graphIds)
        {
            int obstacles = await verticesRepository
                .ReadVerticesByGraphIdAsync(graphId)
                .CountAsync(x => x.IsObstacle, token)
                .ConfigureAwait(false);
            result.Add(graphId, obstacles);
        }
        return result.AsReadOnly();
    }

    public IAsyncEnumerable<Graph> ReadAsync(IReadOnlyCollection<int> ids)
    {
        return Set.Where(x => ids.Contains(x.Id)).ToAsyncEnumerable();
    }

    protected override int NextId()
    {
        return id++;
    }
}