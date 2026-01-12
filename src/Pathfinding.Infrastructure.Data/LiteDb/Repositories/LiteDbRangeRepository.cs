using LiteDB;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.LiteDb.Repositories;

internal sealed class LiteDbRangeRepository : IRangeRepository
{
    private readonly ILiteCollection<PathfindingRange> collection;

    public LiteDbRangeRepository(ILiteDatabase db)
    {
        collection = db.GetCollection<PathfindingRange>(DbTables.Ranges);
        collection.EnsureIndex(x => x.VertexId);
        collection.EnsureIndex(x => x.GraphId);
    }

    public Task<IReadOnlyCollection<PathfindingRange>> CreateAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IReadOnlyCollection<PathfindingRange>>(token);
        }
        collection.Insert(entities);
        return Task.FromResult(entities);
    }

    public Task<bool> DeleteByGraphIdAsync(int graphId,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        int deleted = collection.DeleteMany(x => x.GraphId == graphId);
        return Task.FromResult(deleted > 0);
    }

    public Task<bool> DeleteByVerticesIdsAsync(IReadOnlyCollection<long> verticesIds,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<bool>(token);
        }
        var ids = verticesIds.Select(x => new BsonValue(x)).ToArray();
        var query = Query.In(nameof(PathfindingRange.VertexId), ids);
        return Task.FromResult(collection.DeleteMany(query) > 0);
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdAsync(int graphId)
    {
        return collection.Query()
            .Where(x => x.GraphId == graphId)
            .OrderBy(x => x.Order)
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdsAsync(IReadOnlyCollection<int> ids)
    {
        return collection.Query()
            .Where(x => ids.Contains(x.GraphId))
            .ToEnumerable()
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
        collection.Upsert(entities);
        return Task.FromResult(entities);
    }
}