using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Data.Extensions;

public static class DictionaryExtensions
{
    public static IPathfindingVertex GetOrNullVertex<TKey>(this IReadOnlyDictionary<TKey, IPathfindingVertex> dictionary, TKey key)
    {
        return dictionary.GetValueOrDefault(key, NullPathfindingVertex.Interface);
    }
}