using LiteDB;
using Pathfinding.Domain.Interface;
using Pathfinding.Domain.Interface.Repositories;
using Pathfinding.Infrastructure.Data.LiteDb.Repositories;

namespace Pathfinding.Infrastructure.Data.LiteDb;

internal sealed class LiteDbUnitOfWork : IUnitOfWork
{
    private readonly ILiteDatabase database;

    public IGraphParametersRepository GraphRepository { get; }

    public IVerticesRepository VerticesRepository { get; }

    public IRangeRepository RangeRepository { get; }

    public IStatisticsRepository StatisticsRepository { get; }

    public LiteDbUnitOfWork(string connectionString)
        : this(new ConnectionString(connectionString))
    {
    }

    public LiteDbUnitOfWork(ConnectionString connectionString)
        : this(new LiteDatabase(connectionString))
    {
    }

    public LiteDbUnitOfWork(Stream stream)
        : this(new LiteDatabase(stream))
    {
    }

    public LiteDbUnitOfWork(ILiteDatabase db)
    {
        database = db;
        GraphRepository = new LiteDbGraphRepository(database);
        VerticesRepository = new LiteDbVerticesRepository(database);
        RangeRepository = new LiteDbRangeRepository(database);
        StatisticsRepository = new LiteDbStatisticsRepository(database);
    }

    public Task BeginTransactionAsync(CancellationToken token = default)
    {
        database.BeginTrans();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        database.Dispose();
    }

    public Task RollbackTransactionAsync(CancellationToken token = default)
    {
        database.Rollback();
        return Task.CompletedTask;
    }

    public Task CommitTransactionAsync(CancellationToken token = default)
    {
        database.Commit();
        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync()
    {
        database.Dispose();
        return ValueTask.CompletedTask;
    }
}