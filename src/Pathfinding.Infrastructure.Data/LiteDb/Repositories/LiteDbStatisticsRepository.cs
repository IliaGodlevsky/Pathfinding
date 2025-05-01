using LiteDB;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.LiteDb.Repositories;

internal sealed class LiteDbStatisticsRepository : IStatisticsRepository
{
    private readonly ILiteCollection<Statistics> collection;

    public LiteDbStatisticsRepository(ILiteDatabase db)
    {
        collection = db.GetCollection<Statistics>(DbTables.Statistics);
        collection.EnsureIndex(x => x.GraphId);
        collection.EnsureIndex(x => x.Id);
    }

    public async Task<IReadOnlyCollection<Statistics>> CreateAsync(
        IReadOnlyCollection<Statistics> statistics,
        CancellationToken token = default)
    {
        collection.InsertBulk(statistics);
        return await Task.FromResult(statistics);
    }

    public async Task<bool> DeleteByGraphId(int graphId)
    {
        var deletedCount = collection.DeleteMany(x => x.GraphId == graphId);
        return await Task.FromResult(deletedCount > 0);
    }

    public async Task<bool> DeleteByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken token = default)
    {
        var deletedCount = collection.DeleteMany(x => ids.Contains(x.Id));
        return await Task.FromResult(deletedCount > 0);
    }

    public IAsyncEnumerable<Statistics> ReadByGraphIdAsync(int graphId)
    {
        return collection.Find(x => x.GraphId == graphId).ToAsyncEnumerable();
    }

    public async Task<Statistics> ReadByIdAsync(int id, CancellationToken token = default)
    {
        var result = collection.FindById(id);
        return await Task.FromResult(result);
    }

    public IAsyncEnumerable<Statistics> ReadByIdsAsync(IReadOnlyCollection<int> runIds)
    {
        return collection.Query()
            .Where(x => runIds.Contains(x.Id))
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public async Task<bool> UpdateAsync(
        IReadOnlyCollection<Statistics> entities,
        CancellationToken token = default)
    {
        var updated = collection.Update(entities);
        return await Task.FromResult(updated > 0);
    }
}