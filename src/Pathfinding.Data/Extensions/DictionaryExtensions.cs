using Pathfinding.Service.Interface;

namespace Pathfinding.Data.Extensions;

public static class DictionaryExtensions
{
    public static IPathfindingVertex GetOrNullVertex<TKey>(this IReadOnlyDictionary<TKey, IPathfindingVertex> dictionary, TKey key)
    {
        return dictionary.GetValueOrDefault(key, NullPathfindingVertex.Interface);
    }
}