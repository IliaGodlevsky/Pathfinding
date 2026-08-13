using Pathfinding.Domain;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Data.InMemory.Repositories;

internal sealed class EntityComparer<T, U>
    : Singleton<EntityComparer<T, U>, IEqualityComparer<U>>, IEqualityComparer<U>
    where T : IEquatable<T>
    where U : IEntity<T>
{
    private EntityComparer()
    {

    }

    public bool Equals(U x, U y)
    {
        return x is not null
            && y is not null
            && x.Id.Equals(y.Id);
    }

    public int GetHashCode(U obj)
    {
        return obj.Id.GetHashCode();
    }
}