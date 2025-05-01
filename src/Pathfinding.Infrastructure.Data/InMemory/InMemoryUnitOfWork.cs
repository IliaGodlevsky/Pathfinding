using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Data.InMemory.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory
{
    public sealed class InMemoryUnitOfWork : IUnitOfWork
    {
        public IGraphParametersRepository GraphRepository { get; }

        public IVerticesRepository VerticesRepository { get; }

        public IRangeRepository RangeRepository { get; }

        public IStatisticsRepository StatisticsRepository { get; }

        public InMemoryUnitOfWork()
        {
            var vertices = new InMemoryVerticesRepository();
            var range = new InMemoryRangeRepository();
            var statistics = new InMemoryStatisicsRepository();
            VerticesRepository = vertices;
            RangeRepository = range;
            StatisticsRepository = statistics;
            GraphRepository = new InMemoryGraphParametersRepository(range, vertices, statistics);
        }

        public async Task BeginTransactionAsync(CancellationToken token = default)
        {
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync(CancellationToken token = default)
        {
            await Task.CompletedTask;
        }

        public void Dispose()
        {

        }

        public async Task RollbackTransactionAsync(CancellationToken token = default)
        {
            await Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.CompletedTask;
        }
    }
}
