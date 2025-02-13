using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories
{
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

        public async Task<IEnumerable<Vertex>> CreateAsync(IEnumerable<Vertex> vertices, CancellationToken token = default)
        {
            const string query = @$"
                INSERT INTO {DbTables.Vertices} (GraphId, Coordinates, Cost, UpperValueRange, LowerValueRange, IsObstacle)
                VALUES (@GraphId, @Coordinates, @Cost, @UpperValueRange, @LowerValueRange, @IsObstacle);
                SELECT last_insert_rowid();";

            foreach (var vertex in vertices)
            {
                var id = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(query, vertex, transaction, cancellationToken: token))
                    .ConfigureAwait(false);
                vertex.Id = id;
            }

            return vertices;
        }

        public async Task<bool> DeleteVerticesByGraphIdAsync(int graphId, CancellationToken token = default)
        {
            const string query = $"DELETE FROM {DbTables.Vertices} WHERE GraphId = @GraphId";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(query, new { GraphId = graphId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<Vertex> ReadAsync(long vertexId, CancellationToken token = default)
        {
            const string query = $"SELECT * FROM {DbTables.Vertices} WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<Vertex>(
                new CommandDefinition(query, new { Id = vertexId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Vertex>> ReadVerticesByGraphIdAsync(int graphId, CancellationToken token = default)
        {
            const string query = $"SELECT * FROM {DbTables.Vertices} WHERE GraphId = @GraphId";

            return await connection.QueryAsync<Vertex>(
                new CommandDefinition(query, new { GraphId = graphId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }

        public async Task<bool> UpdateVerticesAsync(IEnumerable<Vertex> vertices, CancellationToken token = default)
        {
            const string query = @$"
                UPDATE {DbTables.Vertices}
                SET Coordinates = @Coordinates,
                    Cost = @Cost,
                    UpperValueRange = @UpperValueRange,
                    LowerValueRange = @LowerValueRange,
                    IsObstacle = @IsObstacle
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(query, vertices, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return affectedRows > 0;
        }
    }
}
