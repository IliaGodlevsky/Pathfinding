using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories;

internal sealed class InMemoryGraphParametersRepository(
    InMemoryRangeRepository rangeRepository,
    InMemoryVerticesRepository verticesRepository,
    InMemoryStatisticsRepository statisticsRepository) : IGraphParametersRepository
{
    private int id;

    private readonly HashSet<Graph> set = new(EntityComparer<int>.Interface);

    public Task<Graph> CreateAsync(Graph graph,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        graph.Id = ++id;
        set.Add(graph);
        return Task.FromResult(graph);
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
        int deleted = set.RemoveWhere(x => x.Id == graphId);
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
        return set.ToAsyncEnumerable();
    }

    public Task<Graph> ReadAsync(int graphId,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var equal = new Graph { Id = graphId };
        set.TryGetValue(equal, out var result);
        return Task.FromResult(result);
    }

    public Task<bool> UpdateAsync(Graph graph,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var equal = new Graph { Id = graph.Id };
        if (set.TryGetValue(equal, out var result))
        {
            result.Dimensions = graph.Dimensions;
            result.Name = graph.Name;
            result.Neighborhood = graph.Neighborhood;
            result.SmoothLevel = graph.SmoothLevel;
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
            var obstacles = await verticesRepository
                .ReadVerticesByGraphIdAsync(graphId)
                .CountAsync(x => x.IsObstacle, token)
                .ConfigureAwait(false);
            result.Add(graphId, obstacles);
        }
        return result;
    }

    public IAsyncEnumerable<Graph> ReadAsync(IReadOnlyCollection<int> ids)
    {
        return set.Where(x => ids.Contains(x.Id)).ToAsyncEnumerable();
    }
}