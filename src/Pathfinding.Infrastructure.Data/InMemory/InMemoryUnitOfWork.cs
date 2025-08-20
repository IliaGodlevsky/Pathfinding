using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Data.InMemory.Repositories;

namespace Pathfinding.Infrastructure.Data.InMemory;

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
        var statistics = new InMemoryStatisticsRepository();
        VerticesRepository = vertices;
        RangeRepository = range;
        StatisticsRepository = statistics;
        GraphRepository = new InMemoryGraphParametersRepository(range, vertices, statistics);
    }

    public ValueTask BeginTransactionAsync(CancellationToken token = default)
    {
        return ValueTask.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {

    }

    public Task RollbackTransactionAsync(CancellationToken token = default)
    {
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}