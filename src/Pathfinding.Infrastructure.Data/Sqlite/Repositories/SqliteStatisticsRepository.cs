using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories
{
    internal sealed class SqliteStatisticsRepository(SqliteConnection connection,
        SqliteTransaction transaction) : SqliteRepository(connection, transaction), IStatisticsRepository
    {
        protected override string CreateTableScript { get; } = $@"
            CREATE TABLE IF NOT EXISTS {DbTables.Statistics} (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                GraphId INTEGER NOT NULL,
                Algorithm INTEGER NOT NULL,
                Heuristics INTEGER,
                Weight REAL,
                StepRule INTEGER,
                ResultStatus INTEGER NOT NULL DEFAULT '',
                Elapsed REAL NOT NULL,
                Steps INTEGER NOT NULL,
                Cost REAL NOT NULL,
                Visited INTEGER NOT NULL,
                FOREIGN KEY (GraphId) REFERENCES {DbTables.Graphs}(Id) ON DELETE CASCADE
            );
            CREATE INDEX IF NOT EXISTS idx_statistics_id ON {DbTables.Statistics}(Id);
            CREATE INDEX IF NOT EXISTS idx_statistics_graphid ON {DbTables.Statistics}(GraphId);";

        public async Task<Statistics> CreateAsync(Statistics entity, CancellationToken token = default)
        {
            const string query = @$"
                INSERT INTO {DbTables.Statistics} (GraphId, Algorithm, Heuristics, StepRule, ResultStatus, Elapsed, Steps, Cost, Visited, Weight)
                VALUES (@GraphId, @Algorithm, @Heuristics, @StepRule, @ResultStatus, @Elapsed, @Steps, @Cost, @Visited, @Weight);
                SELECT last_insert_rowid();";

            var id = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(query, entity, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            entity.Id = id;
            return entity;
        }

        public async Task<IEnumerable<Statistics>> CreateAsync(IEnumerable<Statistics> statistics, CancellationToken token = default)
        {
            const string query = @$"
                INSERT INTO {DbTables.Statistics} (GraphId, Algorithm, Heuristics, StepRule, ResultStatus, Elapsed, Steps, Cost, Visited, Weight)
                VALUES (@GraphId, @Algorithm, @Heuristics, @StepRule, @ResultStatus, @Elapsed, @Steps, @Cost, @Visited, @Weight);
                SELECT last_insert_rowid();";

            foreach (var entity in statistics)
            {
                entity.Id = await connection.ExecuteScalarAsync<int>(
                    new CommandDefinition(query, entity, transaction, cancellationToken: token))
                    .ConfigureAwait(false);
            }

            return statistics;
        }

        public async Task<IEnumerable<Statistics>> ReadByGraphIdAsync(int graphId, CancellationToken token = default)
        {
            const string query = $"SELECT * FROM {DbTables.Statistics} WHERE GraphId = @GraphId";

            return await connection.QueryAsync<Statistics>(
                new CommandDefinition(query, new { GraphId = graphId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }

        public async Task<int> ReadStatisticsCountAsync(int graphId, CancellationToken token = default)
        {
            const string query = $"SELECT COUNT(*) FROM {DbTables.Statistics} WHERE GraphId = @GraphId";

            return await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(query, new { GraphId = graphId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }

        public async Task<bool> DeleteByGraphId(int graphId, CancellationToken token = default)
        {
            const string query = $"DELETE FROM {DbTables.Statistics} WHERE GraphId = @GraphId";

            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(query, new { GraphId = graphId }, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return rowsAffected > 0;
        }

        public async Task<bool> DeleteByIdsAsync(IEnumerable<int> ids, CancellationToken token = default)
        {
            const string query = $"DELETE FROM {DbTables.Statistics} WHERE Id IN @Ids";

            var rowsAffected = await connection.ExecuteAsync(
                new CommandDefinition(query, new { Ids = ids.ToArray() }, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return rowsAffected > 0;
        }

        public async Task<Statistics> ReadByIdAsync(int id, CancellationToken token = default)
        {
            const string query = $"SELECT * FROM {DbTables.Statistics} WHERE Id = @Id";

            var statistics = await connection.QuerySingleOrDefaultAsync<Statistics>(
                new CommandDefinition(query, new { Id = id }, transaction, cancellationToken: token))
                .ConfigureAwait(false);

            return statistics;
        }

        public async Task<bool> UpdateAsync(IEnumerable<Statistics> entities, CancellationToken token = default)
        {
            const string query = @$"
                UPDATE {DbTables.Statistics}
                SET Algorithm = @Algorithm, 
                    Heuristics = @Heuristics, 
                    StepRule = @StepRule, 
                    ResultStatus = @ResultStatus, 
                    Elapsed = @Elapsed, 
                    Steps = @Steps, 
                    Cost = @Cost, 
                    Visited = @Visited, 
                    Weight = @Weight
                WHERE Id = @Id";

            var affectedRows = await connection.ExecuteAsync(
                new CommandDefinition(query, entities.ToArray(), transaction, cancellationToken: token)).ConfigureAwait(false);

            return affectedRows > 0;
        }

        public async Task<IEnumerable<Statistics>> ReadByIdsAsync(IEnumerable<int> runIds, CancellationToken token = default)
        {
            const string query = $"SELECT * FROM {DbTables.Statistics} WHERE Id IN @Ids";

            return await connection.QueryAsync<Statistics>(
                new CommandDefinition(query, new { Ids = runIds.ToArray() }, transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }
    }
}
