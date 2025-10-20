using LiteDB;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.LiteDb.Repositories;

internal sealed class LiteDbGraphRepository : IGraphParametersRepository
{
    private readonly ILiteCollection<Graph> collection;
    private readonly LiteDbRangeRepository rangeRepository;
    private readonly LiteDbVerticesRepository verticesRepository;
    private readonly LiteDbStatisticsRepository statisticsRepository;
    private readonly ILiteCollection<Vertex> vertexCollection;

    public LiteDbGraphRepository(ILiteDatabase db)
    {
        collection = db.GetCollection<Graph>(DbTables.Graphs);
        rangeRepository = new(db);
        verticesRepository = new(db);
        statisticsRepository = new(db);
        vertexCollection = db.GetCollection<Vertex>(DbTables.Vertices);
        collection.EnsureIndex(x => x.Id);
    }

    public Task<Graph> CreateAsync(
        Graph graph, CancellationToken token = default)
    {
        collection.Insert(graph);
        return Task.FromResult(graph);
    }

    public async Task<bool> DeleteAsync(
        int graphId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        // Order sensitive. Do not change the order of deleting
        // Reason: some repositories need the presence of values in the database
        await rangeRepository.DeleteByGraphIdAsync(graphId, token).ConfigureAwait(false);
        await verticesRepository.DeleteVerticesByGraphIdAsync(graphId).ConfigureAwait(false);
        await statisticsRepository.DeleteByGraphId(graphId).ConfigureAwait(false);
        return collection.Delete(graphId);
    }

    public async Task<bool> DeleteAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        foreach (var id in graphIds)
        {
            await DeleteAsync(id, token).ConfigureAwait(false);
        }
        return true;
    }

    public IAsyncEnumerable<Graph> GetAll()
    {
        return collection.FindAll().ToAsyncEnumerable();
    }

    public Task<Graph> ReadAsync(
        int graphId, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(collection.FindById(graphId));
    }

    public Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        var result = vertexCollection
            .Find(x => graphIds.Contains(x.GraphId) && x.IsObstacle)
            .GroupBy(x => x.GraphId)
            .ToDictionary(x => x.Key, x => x.Count());
        return Task.FromResult((IReadOnlyDictionary<int, int>)result);
    }

    public Task<bool> UpdateAsync(Graph graph, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(collection.Update(graph));
    }
}