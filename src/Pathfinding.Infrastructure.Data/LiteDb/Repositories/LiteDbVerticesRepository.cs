using LiteDB;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.LiteDb.Repositories;

internal sealed class LiteDbVerticesRepository : IVerticesRepository
{
    private readonly ILiteCollection<Vertex> collection;

    public LiteDbVerticesRepository(ILiteDatabase db)
    {
        collection = db.GetCollection<Vertex>(DbTables.Vertices);
        collection.EnsureIndex(x => x.Id);
        collection.EnsureIndex(x => x.GraphId);
    }

    public async Task<IReadOnlyCollection<Vertex>> CreateAsync(
        IReadOnlyCollection<Vertex> vertices, 
        CancellationToken token = default)
    {
        collection.InsertBulk(vertices);
        return await Task.FromResult(vertices);
    }

    public async Task<bool> DeleteVerticesByGraphIdAsync(int graphId)
    {
        return await Task.FromResult(collection.DeleteMany(x => x.GraphId == graphId) > 0);
    }

    public async Task<Vertex> ReadAsync(long vertexId,
        CancellationToken token = default)
    {
        return await Task.FromResult(collection.FindById(vertexId));
    }

    public IAsyncEnumerable<Vertex> ReadAsync(
        IReadOnlyCollection<long> verticesIds)
    {
        return collection.Query()
            .Where(x => verticesIds.Contains(x.Id))
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
    {
        return collection.Query()
            .Where(x => x.GraphId == graphId)
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public async Task<bool> UpdateVerticesAsync(
        IReadOnlyCollection<Vertex> vertices, 
        CancellationToken token = default)
    {
        return await Task.FromResult(collection.Update(vertices) > 0);
    }
}