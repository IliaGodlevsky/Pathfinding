using System.Collections.ObjectModel;

namespace Pathfinding.Shared.Extensions
{
    public static class EnumerableExtension
    {
        public static IReadOnlyList<T> ToReadOnly<T>(this IEnumerable<T> collection)
        {
            return collection switch
            {
                ReadOnlyCollection<T> readOnly => readOnly,
                _ => Array.AsReadOnly([..collection]),
            };
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            items.ForEach(collection.Add);
        }

        public static T AggregateOrDefault<T>(this IEnumerable<T> collection, Func<T, T, T> func)
        {
            return collection.Any() ? collection.Aggregate(func) : default;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
            {
                action(item);
            }
            return collection;
        }

        public static IEnumerable<T> ForWhole<T>(this IEnumerable<T> collection, Action<IEnumerable<T>> action)
        {
            action(collection);
            return collection;
        }

        public static IEnumerable<T> TakeOrDefault<T>(this IEnumerable<T> collection, int number, T defaultValue = default)
        {
            int count = 0;
            foreach (var item in collection)
            {
                count++;
                if (count > number)
                {
                    break;
                }
                yield return item;
            }
            int remained = count != number ? number - count : 0;
            while (remained-- > 0)
            {
                yield return defaultValue;
            }
        }

        public static async Task<U> ToAsync<T, U>(this IEnumerable<T> items,
            Func<IEnumerable<T>, CancellationToken, Task<U>> selector,
            CancellationToken token = default)
        {
            return await selector(items, token);
        }

        public static U To<T, U>(this IEnumerable<T> items, Func<IEnumerable<T>, U> selector)
        {
            return selector(items);
        }
    }
}
