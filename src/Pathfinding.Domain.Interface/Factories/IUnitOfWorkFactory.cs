namespace Pathfinding.Domain.Interface.Factories;

public interface IUnitOfWorkFactory
{
    Task<IUnitOfWork> CreateAsync(CancellationToken token = default);
}
