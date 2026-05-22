using Pathfinding.Domain.Interface;

namespace Pathfinding.Data.LiteDb;

public sealed class LiteDbInMemoryUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly MemoryStream Memory = new();

    public Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IUnitOfWork>(token);
        }
        IUnitOfWork uow = new LiteDbUnitOfWork(Memory);
        return Task.FromResult(uow);
    }
}