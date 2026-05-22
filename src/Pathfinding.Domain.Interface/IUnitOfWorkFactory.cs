namespace Pathfinding.Domain.Interface;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateAsync(CancellationToken token = default);
}
