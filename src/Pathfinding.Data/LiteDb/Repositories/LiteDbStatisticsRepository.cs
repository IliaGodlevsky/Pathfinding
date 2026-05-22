using LiteDB;
using Pathfinding.Domain.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Data.LiteDb.Repositories;

internal sealed class LiteDbStatisticsRepository : IStatisticsRepository
{
    private readonly ILiteCollection<Statistics> collection;

    public LiteDbStatisticsRepository(ILiteDatabase db)
    {
        collection = db.GetCollection<Statistics>(DbTables.Statistics);
        collection.EnsureIndex(x => x.GraphId);
        collection.EnsureIndex(x => x.Id);
    }

    public Task<IReadOnlyCollection<Statistics>> CreateAsync(
        IReadOnlyCollection<Statistics> statistics,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IReadOnlyCollection<Statistics>>(token);
        }
        collection.InsertBulk(statistics);
        return Task.FromResult(statistics);
    }

    public Task<bool> DeleteByGraphId(int graphId)
    {
        var deletedCount = collection.DeleteMany(x => x.GraphId == graphId);
        return Task.FromResult(deletedCount > 0);
    }

    public Task<bool> DeleteByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var deletedCount = collection.DeleteMany(x => ids.Contains(x.Id));
        return Task.FromResult(deletedCount > 0);
    }

    public IAsyncEnumerable<Statistics> ReadByGraphIdAsync(int graphId, int skip, int take)
    {
        return collection
            .Find(x => x.GraphId == graphId)
            .Skip(skip)
            .Take(take)
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Statistics> ReadByGraphIdsAsync(IReadOnlyCollection<int> graphIds)
    {
        return collection.Query()
            .Where(x => graphIds.Contains(x.GraphId))
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public Task<Statistics> ReadByIdAsync(int id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<Statistics>(token);
        }
        var result = collection.FindById(id);
        return Task.FromResult(result);
    }

    public IAsyncEnumerable<Statistics> ReadByIdsAsync(IReadOnlyCollection<int> runIds)
    {
        return collection.Query()
            .Where(x => runIds.Contains(x.Id))
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public Task<int> ReadCountAsync(int graphId, CancellationToken token = default)
    {
        var result = collection.Query()
            .Where(x => x.GraphId == graphId)
            .Count();
        return Task.FromResult(result);
    }

    public Task<bool> UpdateAsync(
        IReadOnlyCollection<Statistics> entities,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var updated = collection.Update(entities);
        return Task.FromResult(updated > 0);
    }
}