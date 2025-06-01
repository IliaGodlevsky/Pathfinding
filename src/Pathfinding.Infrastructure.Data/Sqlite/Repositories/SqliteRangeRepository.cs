using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories;

internal sealed class SqliteRangeRepository(SqliteConnection connection,
    SqliteTransaction transaction) : SqliteRepository(connection, transaction), IRangeRepository
{
    protected override string CreateTableScript { get; } = @$"
            CREATE TABLE IF NOT EXISTS {DbTables.Ranges} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IsSource BOOLEAN NOT NULL,
                IsTarget BOOLEAN NOT NULL,
                GraphId INTEGER NOT NULL,
                VertexId INTEGER NOT NULL,
                ""Order"" INTEGER NOT NULL,
                FOREIGN KEY (GraphId) REFERENCES {DbTables.Graphs}(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_range_vertexid ON {DbTables.Ranges}(VertexId);
            CREATE INDEX IF NOT EXISTS idx_range_graphid ON {DbTables.Ranges}(GraphId);";

    public async Task<IReadOnlyCollection<PathfindingRange>> CreateAsync(
        IReadOnlyCollection<PathfindingRange> entities,
        CancellationToken token = default)
    {
        const string query = @$"
                INSERT INTO {DbTables.Ranges} (IsSource, IsTarget, GraphId, VertexId, ""Order"")
                VALUES (@IsSource, @IsTarget, @GraphId, @VertexId, @Order);
                SELECT last_insert_rowid();";

        foreach (var entity in entities)
        {
            var id = await Connection.ExecuteScalarAsync<int>(
                    new(query, entity, Transaction, cancellationToken: token))
                .ConfigureAwait(false);
            entity.Id = id;
        }

        return entities;
    }

    public async Task<bool> DeleteByGraphIdAsync(int graphId,
        CancellationToken token = default)
    {
        const string query = $"DELETE FROM {DbTables.Ranges} WHERE GraphId = @GraphId";

        var affectedRows = await Connection.ExecuteAsync(
                new(query, new { GraphId = graphId }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return affectedRows > 0;
    }

    public async Task<bool> DeleteByVerticesIdsAsync(
        IReadOnlyCollection<long> verticesIds, 
        CancellationToken token = default)
    {
        const string query = $"DELETE FROM {DbTables.Ranges} WHERE VertexId IN @VerticesIds";

        var affectedRows = await Connection.ExecuteAsync(
                new(query, new { VerticesIds = verticesIds.ToArray() }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return affectedRows > 0;
    }

    public IAsyncEnumerable<PathfindingRange> ReadByGraphIdAsync(int graphId)
    {
        const string query = $"SELECT * FROM {DbTables.Ranges} WHERE GraphId = @GraphId ORDER BY \"Order\"";
        var parameters = new { GraphId = graphId };
        return Connection.QueryUnbufferedAsync<PathfindingRange>(query, 
            parameters, Transaction);
    }

    public async Task<IReadOnlyCollection<PathfindingRange>> UpsertAsync(
        IReadOnlyCollection<PathfindingRange> entities, 
        CancellationToken token = default)
    {
        const string updateQuery = @$"
                UPDATE {DbTables.Ranges}
                SET IsSource = @IsSource,
                    IsTarget = @IsTarget,
                    GraphId = @GraphId,
                    VertexId = @VertexId,
                    ""Order"" = @Order
                WHERE Id = @Id";

        await Connection.ExecuteAsync(
                new(updateQuery, entities.Where(e => e.Id > 0).ToArray(), Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        const string insertQuery = @$"
                INSERT INTO {DbTables.Ranges} 
                    (IsSource, IsTarget, GraphId, VertexId, ""Order"")
                    VALUES (@IsSource, @IsTarget, @GraphId, @VertexId, @Order); 
                    SELECT last_insert_rowid();";

        foreach (var entity in entities.Where(e => e.Id == 0))
        {
            var newId = await Connection.ExecuteScalarAsync<int>(
                    new(insertQuery, entity, Transaction, cancellationToken: token))
                .ConfigureAwait(false);

            entity.Id = newId;
        }
        return entities;
    }
}