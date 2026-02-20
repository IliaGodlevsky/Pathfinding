using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.InMemory;

public sealed class InMemoryUnitOfWorkFactory : IUnitOfWorkFactory
{
    private static readonly IUnitOfWork unitOfWork = new InMemoryUnitOfWork();

    public Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        return token.IsCancellationRequested
            ? Task.FromCanceled<IUnitOfWork>(token)
            : Task.FromResult(unitOfWork);
    }
}