namespace Pathfinding.Shared.Extensions;

public static class GenericExtensions
{
    public static IEnumerable<T> Enumerate<T>(this T obj)
    {
        yield return obj;
    }
}