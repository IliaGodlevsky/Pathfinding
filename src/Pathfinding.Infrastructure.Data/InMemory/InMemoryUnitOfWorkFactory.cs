using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Infrastructure.Data.InMemory;

public sealed class InMemoryUnitOfWorkFactory : IUnitOfWorkFactory
{
    private readonly InMemoryUnitOfWork unitOfWork = new();

    public IUnitOfWork Create() => unitOfWork;
}