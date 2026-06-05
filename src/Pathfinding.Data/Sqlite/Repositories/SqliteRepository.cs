using Dapper;
using Microsoft.Data.Sqlite;

namespace Pathfinding.Data.Sqlite.Repositories;

internal abstract class SqliteRepository(
    SqliteConnection connection,
    SqliteTransaction transaction)
{
    protected readonly SqliteConnection Connection = connection;
    protected readonly SqliteTransaction Transaction = transaction;
}