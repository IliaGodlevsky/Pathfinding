using Pathfinding.Domain.Interface.Factories;

namespace Pathfinding.Domain.Interface.Extensions;

public static class UnitOfWorkFactoryExtensions
{
    public static async Task<TParam> TransactionAsync<TParam>(this IUnitOfWorkFactory factory,
        Func<IUnitOfWork, CancellationToken, Task<TParam>> action,
        CancellationToken token)
    {
        await using var unitOfWork = factory.Create();
        try
        {
            await unitOfWork.BeginTransactionAsync(token).ConfigureAwait(false);
            var result = await action(unitOfWork, token).ConfigureAwait(false);
            await unitOfWork.CommitTransactionAsync(token).ConfigureAwait(false);
            return result;
        }
        catch (Exception)
        {
            await unitOfWork.RollbackTransactionAsync(token).ConfigureAwait(false);
            throw;
        }
    }
}