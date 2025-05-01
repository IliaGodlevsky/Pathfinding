using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories
{
    internal sealed class SqliteGraphRepository : SqliteRepository, IGraphParametersRepository
    {
        protected override string CreateTableScript { get; } = @$"
            CREATE TABLE IF NOT EXISTS {DbTables.Graphs} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL,
                Neighborhood INTEGER NOT NULL,
                SmoothLevel INTEGER NOT NULL,
                Status INTEGER NOT NULL,
                Dimensions TEXT NOT NULL
            );
            CREATE INDEX IF NOT EXISTS idx_graph_id ON {DbTables.Graphs}(Id);";

        public SqliteGraphRepository(SqliteConnection connection,
            SqliteTransaction transaction) : base(connection, transaction)
        {
            _ = new SqliteVerticesRepository(connection, transaction);
        }

        public async Task<Graph> CreateAsync(Graph graph, CancellationToken token = default)
        {
            const string query = @$"
                INSERT INTO {DbTables.Graphs} (Name, Neighborhood, SmoothLevel, Status, Dimensions)
                VALUES (@Name, @Neighborhood, @SmoothLevel, @Status, @Dimensions);
                SELECT last_insert_rowid();";

            var id = await connection.ExecuteScalarAsync<int>(
                new(query, graph, transaction, cancellationToken: token))
                .ConfigureAwait(false);
            graph.Id = id;
            return graph;
        }

        public async Task<bool> DeleteAsync(
            IReadOnlyCollection<int> graphIds, 
            CancellationToken token = default)
        {
            const string query = $"DELETE FROM {DbTables.Graphs} WHERE Id IN @Ids";

            var affectedRows = await connection.ExecuteAsync(
                new(query, new { Ids = graphIds }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
            return affectedRows > 0;
        }

        public IAsyncEnumerable<Graph> GetAll()
        {
            const string query = $"SELECT * FROM {DbTables.Graphs}";

            return connection.QueryUnbufferedAsync<Graph>(query, transaction: transaction);
        }

        public async Task<Graph> ReadAsync(int graphId, CancellationToken token = default)
        {
            const string query = $"SELECT * FROM {DbTables.Graphs} WHERE Id = @Id";

            return await connection.QuerySingleOrDefaultAsync<Graph>(
                new(query, new { Id = graphId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }

        public async Task<bool> UpdateAsync(Graph graph, CancellationToken token = default)
        {
            const string query = @$"
                UPDATE {DbTables.Graphs}
                SET Name = @Name,
                    Neighborhood = @Neighborhood,
                    SmoothLevel = @SmoothLevel,
                    Status = @Status,
                    Dimensions = @Dimensions
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(
                new(query, graph, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(
            IReadOnlyCollection<int> graphIds,
            CancellationToken token = default)
        {
            const string query = $@"
                SELECT g.GraphId, COALESCE(SUM(v.IsObstacle), 0) AS ObstacleCount
                FROM (SELECT DISTINCT GraphId FROM Vertices WHERE GraphId IN @GraphIds) g
                LEFT JOIN Vertices v ON g.GraphId = v.GraphId
                GROUP BY g.GraphId;";

            var result = await connection.QueryAsync<(int GraphId, int ObstacleCount)>(
                new(query, new { GraphIds = graphIds }, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return result.ToDictionary(x => x.GraphId, x => x.ObstacleCount);
        }
    }
}
