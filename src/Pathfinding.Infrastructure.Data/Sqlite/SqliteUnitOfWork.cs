using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Data.Sqlite.Repositories;

namespace Pathfinding.Infrastructure.Data.Sqlite;

public sealed class SqliteUnitOfWork : IUnitOfWork
{
    private readonly SqliteConnection connection;
    private SqliteTransaction transaction;

    public IGraphParametersRepository GraphRepository
        => new SqliteGraphRepository(connection, transaction);

    public IVerticesRepository VerticesRepository
        => new SqliteVerticesRepository(connection, transaction);

    public IRangeRepository RangeRepository
        => new SqliteRangeRepository(connection, transaction);

    public IStatisticsRepository StatisticsRepository
        => new SqliteStatisticsRepository(connection, transaction);

    public SqliteUnitOfWork(string connectionString)
    {
        connection = new(connectionString);
        connection.Open();
    }

    public async ValueTask BeginTransactionAsync(CancellationToken token = default)
    {
        transaction ??= (SqliteTransaction)await connection
            .BeginTransactionAsync(token)
            .ConfigureAwait(false);
    }

    public async Task CommitTransactionAsync(CancellationToken token = default)
    {
        if (transaction is not null)
        {
            await transaction.CommitAsync(token).ConfigureAwait(false);
            await transaction.DisposeAsync().ConfigureAwait(false);
            transaction = null;
        }
    }

    public void Dispose()
    {
        transaction?.Dispose();
        connection.Dispose();
    }

    public async Task RollbackTransactionAsync(CancellationToken token = default)
    {
        if (transaction is not null)
        {
            await transaction.RollbackAsync(token).ConfigureAwait(false);
            await transaction.DisposeAsync().ConfigureAwait(false);
            transaction = null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (transaction is not null)
        {
            await transaction.DisposeAsync().ConfigureAwait(false);
        }
        await connection.DisposeAsync().ConfigureAwait(false);
    }
}