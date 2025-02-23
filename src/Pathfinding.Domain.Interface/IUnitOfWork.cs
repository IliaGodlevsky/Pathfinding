using Pathfinding.Domain.Interface.Repositories;

namespace Pathfinding.Domain.Interface
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        IGraphParametresRepository GraphRepository { get; }

        IVerticesRepository VerticesRepository { get; }

        IRangeRepository RangeRepository { get; }

        IStatisticsRepository StatisticsRepository { get; }

        Task BeginTransactionAsync(CancellationToken token = default);

        Task RollbackTransactionAsync(CancellationToken token = default);

        Task CommitTransactionAsync(CancellationToken token = default);
    }
}
