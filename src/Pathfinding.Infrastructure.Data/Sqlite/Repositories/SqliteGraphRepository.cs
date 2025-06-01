using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Core.Entities;
using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories;

internal sealed class SqliteGraphRepository : SqliteRepository, IGraphParametersRepository
{
    private const string NameProperty = nameof(Graph.Name);
    private const string NeighborhoodProperty = nameof(Graph.Neighborhood);
    private const string SmoothLevelProperty = nameof(Graph.SmoothLevel);
    private const string StatusProperty = nameof(Graph.Status);
    private const string DimensionsProperty = nameof(Graph.Dimensions);
    private const string IdProperty = nameof(Graph.Id);

    protected override string CreateTableScript =>
        @$"
            CREATE TABLE IF NOT EXISTS {DbTables.Graphs} (
                {IdProperty} INTEGER PRIMARY KEY AUTOINCREMENT,
                {NameProperty} TEXT NOT NULL,
                {NeighborhoodProperty} INTEGER NOT NULL,
                {SmoothLevelProperty} INTEGER NOT NULL,
                {StatusProperty} INTEGER NOT NULL,
                {DimensionsProperty} TEXT NOT NULL
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
                INSERT INTO {DbTables.Graphs} ({NameProperty}, {NeighborhoodProperty}, {SmoothLevelProperty}, {StatusProperty}, {DimensionsProperty})
                VALUES (@{NameProperty}, @{NeighborhoodProperty}, @{SmoothLevelProperty}, @{StatusProperty}, @{DimensionsProperty});
                SELECT last_insert_rowid();";

        var id = await Connection.ExecuteScalarAsync<int>(
                new(query, graph, Transaction, cancellationToken: token))
            .ConfigureAwait(false);
        graph.Id = id;
        return graph;
    }

    public async Task<bool> DeleteAsync(
        IReadOnlyCollection<int> graphIds, 
        CancellationToken token = default)
    {
        const string query = $"DELETE FROM {DbTables.Graphs} WHERE Id IN @Ids";

        var affectedRows = await Connection.ExecuteAsync(
                new(query, new { Ids = graphIds }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);
        return affectedRows > 0;
    }

    public IAsyncEnumerable<Graph> GetAll()
    {
        const string query = $"SELECT * FROM {DbTables.Graphs}";

        return Connection.QueryUnbufferedAsync<Graph>(query, transaction: Transaction);
    }

    public async Task<Graph> ReadAsync(int graphId, CancellationToken token = default)
    {
        const string query = $"SELECT * FROM {DbTables.Graphs} WHERE Id = @Id";

        return await Connection.QuerySingleOrDefaultAsync<Graph>(
                new(query, new { Id = graphId }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);
    }

    public async Task<bool> UpdateAsync(Graph graph, CancellationToken token = default)
    {
        const string query = @$"
                UPDATE {DbTables.Graphs}
                SET {NameProperty} = @{NameProperty},
                    {NeighborhoodProperty} = @{NeighborhoodProperty},
                    {SmoothLevelProperty} = @{SmoothLevelProperty},
                    {StatusProperty} = @{StatusProperty},
                    {DimensionsProperty} = @{DimensionsProperty}
                WHERE {IdProperty} = @{IdProperty}";

        var affectedRows = await Connection.ExecuteAsync(
                new(query, graph, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return affectedRows > 0;
    }

    public async Task<IReadOnlyDictionary<int, int>> ReadObstaclesCountAsync(
        IReadOnlyCollection<int> graphIds,
        CancellationToken token = default)
    {
        const string graphIdProperty = nameof(Vertex.GraphId);

        const string query = $@"
                SELECT g.{graphIdProperty}, COALESCE(SUM(v.{nameof(Vertex.IsObstacle)}), 0) AS ObstacleCount
                FROM (SELECT DISTINCT {graphIdProperty} FROM {DbTables.Vertices} WHERE {graphIdProperty} IN @GraphIds) g
                LEFT JOIN { DbTables.Vertices } v ON g.{graphIdProperty} = v.{graphIdProperty}
                GROUP BY g.{graphIdProperty};";

        var result = await Connection.QueryAsync<(int GraphId, int ObstacleCount)>(
                new(query, new { GraphIds = graphIds }, Transaction, cancellationToken: token))
            .ConfigureAwait(false);

        return result.ToDictionary(x => x.GraphId, x => x.ObstacleCount);
    }
}