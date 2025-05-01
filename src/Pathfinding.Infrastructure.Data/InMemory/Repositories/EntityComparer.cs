using Pathfinding.Domain.Core;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Data.InMemory.Repositories
{
    internal sealed class EntityComparer<T>
        : Singleton<EntityComparer<T>, IEqualityComparer<IEntity<T>>>, IEqualityComparer<IEntity<T>>
    {
        private EntityComparer()
        {

        }

        public bool Equals(IEntity<T> x, IEntity<T> y)
        {
            if (x is not null && y is not null)
            {
                return x.Id.Equals(y.Id);
            }
            return false;
        }

        public int GetHashCode(IEntity<T> obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
