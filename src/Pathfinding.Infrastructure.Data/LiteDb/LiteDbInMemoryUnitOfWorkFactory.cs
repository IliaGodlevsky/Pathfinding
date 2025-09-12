using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.LiteDb;

public sealed class LiteDbInMemoryUnitOfWorkFactory : IUnitOfWorkFactory
{
    private static readonly MemoryStream Memory = new();

    public Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        IUnitOfWork uow = new LiteDbUnitOfWork(Memory);
        return Task.FromResult(uow);
    }
}