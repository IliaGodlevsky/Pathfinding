﻿using LiteDB;
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

    public async Task<IReadOnlyCollection<PathfindingRange>> CreateAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default)
    {
        collection.Insert(entities);
        return await Task.FromResult(entities);
    }

    public async Task<bool> DeleteByGraphIdAsync(int graphId, 
        CancellationToken token = default)
    {
        int deleted = collection.DeleteMany(x => x.GraphId == graphId);
        return await Task.FromResult(deleted > 0);
    }

    public async Task<bool> DeleteByVerticesIdsAsync(IReadOnlyCollection<long> verticesIds,
        CancellationToken token = default)
    {
        var ids = verticesIds.Select(x => new BsonValue(x)).ToArray();
        var query = Query.In(nameof(PathfindingRange.VertexId), ids);
        return await Task.FromResult(collection.DeleteMany(query) > 0);
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdAsync(int graphId)
    {
        return collection.Query()
            .Where(x => x.GraphId == graphId)
            .OrderBy(x => x.Order)
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public async Task<IReadOnlyCollection<PathfindingRange>> UpsertAsync(
        IReadOnlyCollection<PathfindingRange> entities, 
        CancellationToken token = default)
    {
        collection.Upsert(entities);
        return await Task.FromResult(entities);
    }
}