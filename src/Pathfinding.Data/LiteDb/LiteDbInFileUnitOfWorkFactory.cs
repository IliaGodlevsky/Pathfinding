using LiteDB;
using Pathfinding.Domain.Interface;

namespace Pathfinding.Data.LiteDb;

public sealed class LiteDbInFileUnitOfWorkFactory(ConnectionString connectionString) : IUnitOfWorkFactory
{
    public LiteDbInFileUnitOfWorkFactory(string connectionString)
        : this(new ConnectionString(connectionString))
    {

    }

    public Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IUnitOfWork>(token);
        }
        IUnitOfWork uow = new LiteDbUnitOfWork(connectionString);
        return Task.FromResult(uow);
    }
}