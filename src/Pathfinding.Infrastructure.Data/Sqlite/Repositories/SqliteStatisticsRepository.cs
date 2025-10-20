using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories;

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

    public async Task<IReadOnlyCollection<Statistics>> CreateAsync(
        IReadOnlyCollection<Statistics> statistics,
        CancellationToken token = default)
    {
        const string query = @$"
                INSERT INTO {DbTables.Statistics} (GraphId, Algorithm, Heuristics, StepRule, ResultStatus, Elapsed, Steps, Cost, Visited, Weight)
                VALUES (@GraphId, @Algorithm, @Heuristics, @StepRule, @ResultStatus, @Elapsed, @Steps, @Cost, @Visited, @Weight);
                SELECT last_insert_rowid();";

        foreach (var entity in statistics)
        {
            entity.Id = await Connection.ExecuteScalarAsync<int>(
                    new(query, entity, Transaction, cancellationToken: token))
                .ConfigureAwait(false);
        }

        return statistics;
    }

    public IAsyncEnumerable<Statistics> ReadByGraphIdAsync(int graphId)
    {
        const string query = $"SELECT * FROM {DbTables.Statistics} WHERE GraphId = @GraphId";

        return Connection.QueryUnbufferedAsync<Statistics>(query,
            new { GraphId = graphId }, Transaction);
    }

    public async Task<bool> DeleteByIdsAsync(
        IReadOnlyCollection<int> ids, CancellationToken token = default)
    {
        const string query = $"DELETE FROM {DbTables.Statistics} WHERE Id IN @Ids";

        var rowsAffected = await Connection.ExecuteAsync(
                new(query, new { Ids = ids.ToArray() }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return rowsAffected > 0;
    }

    public async Task<Statistics> ReadByIdAsync(
        int id, CancellationToken token = default)
    {
        const string query = $"SELECT * FROM {DbTables.Statistics} WHERE Id = @Id";

        var statistics = await Connection.QuerySingleOrDefaultAsync<Statistics>(
                new(query, new { Id = id }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return statistics;
    }

    public async Task<bool> UpdateAsync(
        IReadOnlyCollection<Statistics> entities,
        CancellationToken token = default)
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

        var affectedRows = await Connection.ExecuteAsync(
            new(query, entities.ToArray(), Transaction, cancellationToken: token)).ConfigureAwait(false);

        return affectedRows > 0;
    }

    public IAsyncEnumerable<Statistics> ReadByIdsAsync(IReadOnlyCollection<int> runIds)
    {
        const string query = $"SELECT * FROM {DbTables.Statistics} WHERE Id IN @Ids";

        return Connection.QueryUnbufferedAsync<Statistics>(query,
            new { Ids = runIds.ToArray() }, Transaction);
    }
}