using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories;

internal sealed class SqliteVerticesRepository(SqliteConnection connection,
    SqliteTransaction transaction) : SqliteRepository(connection, transaction), IVerticesRepository
{
    protected override string CreateTableScript { get; } =
        @$"CREATE TABLE IF NOT EXISTS {DbTables.Vertices} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GraphId INTEGER NOT NULL,
                Coordinates TEXT NOT NULL,
                Cost INTEGER NOT NULL,
                UpperValueRange INTEGER NOT NULL,
                LowerValueRange INTEGER NOT NULL,
                IsObstacle BOOLEAN NOT NULL,
                FOREIGN KEY (GraphId) REFERENCES {DbTables.Graphs}(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_vertex_id ON {DbTables.Vertices}(Id);
            CREATE INDEX IF NOT EXISTS idx_vertex_graphid ON {DbTables.Vertices}(GraphId);";

    public async Task<IReadOnlyCollection<Vertex>> CreateAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        const string query = @$"
                INSERT INTO {DbTables.Vertices} (GraphId, Coordinates, Cost, UpperValueRange, LowerValueRange, IsObstacle)
                VALUES (@GraphId, @Coordinates, @Cost, @UpperValueRange, @LowerValueRange, @IsObstacle);
                SELECT last_insert_rowid();";

        foreach (var vertex in vertices)
        {
            var id = await Connection.ExecuteScalarAsync<int>(
                    new(query, vertex, Transaction, cancellationToken: token))
                .ConfigureAwait(false);
            vertex.Id = id;
        }

        return vertices;
    }

    public async Task<Vertex> ReadAsync(
        long vertexId, CancellationToken token = default)
    {
        const string query = $"SELECT * FROM {DbTables.Vertices} WHERE Id = @Id";

        return await Connection.QuerySingleOrDefaultAsync<Vertex>(
                new(query, new { Id = vertexId }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdAsync(int graphId)
    {
        const string query = $"SELECT * FROM {DbTables.Vertices} WHERE GraphId = @GraphId";

        return Connection.QueryUnbufferedAsync<Vertex>(query, new { GraphId = graphId });
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByGraphIdsAsync(IReadOnlyCollection<int> graphIds)
    {
        const string query = $"SELECT * FROM {DbTables.Vertices} WHERE GraphId IN @Ids";
        return connection.QueryUnbufferedAsync<Vertex>(query, new { Ids = graphIds }, transaction: Transaction);
    }

    public IAsyncEnumerable<Vertex> ReadVerticesByIdsAsync(IReadOnlyCollection<long> vertexIds)
    {
        const string query = $"SELECT * FROM {DbTables.Vertices} WHERE Id IN @Ids";
        return Connection.QueryUnbufferedAsync<Vertex>(query, new { Ids = vertexIds });
    }

    public async Task<bool> UpdateVerticesAsync(
        IReadOnlyCollection<Vertex> vertices,
        CancellationToken token = default)
    {
        const string query = @$"
                UPDATE {DbTables.Vertices}
                SET Coordinates = @Coordinates,
                    Cost = @Cost,
                    UpperValueRange = @UpperValueRange,
                    LowerValueRange = @LowerValueRange,
                    IsObstacle = @IsObstacle
                WHERE Id = @Id";

        var affectedRows = await Connection.ExecuteAsync(
                new(query, vertices, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return affectedRows > 0;
    }
}