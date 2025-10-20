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

    public Task<IReadOnlyCollection<Vertex>> CreateAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        collection.InsertBulk(vertices);
        return Task.FromResult(vertices);
    }

    public Task<bool> DeleteVerticesByGraphIdAsync(int graphId)
    {
        return Task.FromResult(collection.DeleteMany(x => x.GraphId == graphId) > 0);
    }

    public Task<Vertex> ReadAsync(long vertexId,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(collection.FindById(vertexId));
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
    {
        return collection.Query()
            .Where(x => x.GraphId == graphId)
            .ToEnumerable()
            .ToAsyncEnumerable();
    }

    public Task<bool> UpdateVerticesAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();
        return Task.FromResult(collection.Update(vertices) > 0);
    }
}