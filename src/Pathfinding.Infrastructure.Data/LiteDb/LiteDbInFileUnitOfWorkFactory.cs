using LiteDB;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.LiteDb;

public sealed class LiteDbInFileUnitOfWorkFactory(ConnectionString connectionString) : IUnitOfWorkFactory
{
    public LiteDbInFileUnitOfWorkFactory(string connectionString)
        : this(new ConnectionString(connectionString))
    {

    }

    public Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        IUnitOfWork uow = new LiteDbUnitOfWork(connectionString);
        return Task.FromResult(uow);
    }
}