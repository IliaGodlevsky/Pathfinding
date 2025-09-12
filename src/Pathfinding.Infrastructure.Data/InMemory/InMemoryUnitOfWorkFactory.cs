using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.InMemory;

public sealed class InMemoryUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly IUnitOfWork unitOfWork = new InMemoryUnitOfWork();

    public Task<IUnitOfWork> CreateAsync(CancellationToken token = default)
    {
        return Task.FromResult(unitOfWork);
    }
}