using Dapper;
using Microsoft.Data.Sqlite;

namespace Pathfinding.Infrastructure.Data.Sqlite.Repositories;

internal abstract class SqliteRepository
{
    protected readonly SqliteConnection Connection;
    protected readonly SqliteTransaction Transaction;

    protected abstract string CreateTableScript { get; }

    protected SqliteRepository(
        SqliteConnection connection,
        SqliteTransaction transaction)
    {
        Connection = connection;
        Transaction = transaction;
        connection.Execute(CreateTableScript);
    }
}