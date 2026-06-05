using Dapper;
using Microsoft.Data.Sqlite;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Data.Sqlite;

public sealed class SqliteUnitOfWorkFactory(string connectionString) : IUnitOfWorkFactory
{
    public async Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        var unitOfWork = new SqliteUnitOfWork(connectionString);
        await unitOfWork.OpenConnectionAsync(token).ConfigureAwait(false);
        return unitOfWork;
    }

    public void CreateTables()
    {
        using SqliteConnection connection = new(connectionString);
        connection.Open();
        using var transaction = connection.BeginTransaction();
        try
        {
            string script = SqliteUnitOfWork.GetTablesCreationScript();
            connection.Execute(new(script, transaction: transaction));
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }
}