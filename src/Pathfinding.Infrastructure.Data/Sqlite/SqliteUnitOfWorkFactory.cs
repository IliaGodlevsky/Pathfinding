using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.Sqlite;

public sealed class SqliteUnitOfWorkFactory(string connectionString) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        var unitOfWork = new SqliteUnitOfWork(connectionString);
        await unitOfWork.OpenConnectionAsync(token).ConfigureAwait(false);
        return unitOfWork;
    }
}