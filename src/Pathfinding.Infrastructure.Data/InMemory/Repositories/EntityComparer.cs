using Pathfinding.Domain.Core;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories;

internal sealed class EntityComparer<T>
    : Singleton<EntityComparer<T>, IEqualityComparer<IEntity<T>>>, IEqualityComparer<IEntity<T>>
    where T : IEquatable<T>
{
    private EntityComparer()
    {

    }

    public bool Equals(IEntity<T> x, IEntity<T> y)
    {
        return x is not null
               && y is not null
               && x.Id.Equals(y.Id);
    }

    public int GetHashCode(IEntity<T> obj)
    {
        return obj.Id.GetHashCode();
    }
}