using Pathfinding.Domain;

namespace Pathfinding.Data.InMemory.Repositories;

public abstract class InMemoryRepository<T, U>
    where U : IEntity<T>, new()
    where T : struct, IEquatable<T>
{
    protected T id;
    protected readonly HashSet<U> Set = new(EntityComparer<T, U>.Instance);

    protected abstract T NextId();

    public virtual Task<IReadOnlyCollection<U>> CreateAsync(IReadOnlyCollection<U> entities,
        CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<IReadOnlyCollection<U>>(token);
        }
        foreach (var entity in entities)
        {
            entity.Id = NextId();
            Set.Add(entity);
        }
        return Task.FromResult(entities);
    }

    public virtual Task<U> ReadAsync(T id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested)
        {
            return Task.FromCanceled<U>(token);
        }
        var entity = new U { Id = id };
        Set.TryGetValue(entity, out var result);
        return Task.FromResult(result);
    }
}
