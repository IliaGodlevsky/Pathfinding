using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.Sqlite;

public sealed class SqliteUnitOfWorkFactory(string connectionString) : IUnitOfWorkFactory
{
    public IUnitOfWork Create() => new SqliteUnitOfWork(connectionString);
}