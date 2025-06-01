using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Shared;

public sealed class BlackHole<T>
    : Singleton<BlackHole<T>, ICollection<T>>, ICollection<T>, IReadOnlyCollection<T>
{
    public int Count => default;

    public bool IsReadOnly => default;

    private BlackHole()
    {

    }

    public void Add(T item)
    {

    }

    public void Clear()
    {

    }

    public bool Contains(T item)
    {
        return default;
    }

    public void CopyTo(T[] array, int arrayIndex)
    {

    }

    public IEnumerator<T> GetEnumerator()
    {
        return Enumerable.Empty<T>().GetEnumerator();
    }

    public bool Remove(T item)
    {
        return default;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}