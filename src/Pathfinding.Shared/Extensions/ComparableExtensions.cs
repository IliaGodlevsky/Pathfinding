namespace Pathfinding.Shared.Extensions;

public static class ComparableExtensions
{
    public static bool IsGreaterThan<T>(this T first, T second)
        where T : IComparable<T>
    {
        return first.CompareTo(second) > 0;
    }

    public static bool IsLessThan<T>(this T first, T second)
        where T : IComparable<T>
    {
        return first.CompareTo(second) < 0;
    }

    public static bool IsGreaterOrEqualThan<T>(this T first, T second)
        where T : IComparable<T>
    {
        return first.CompareTo(second) >= 0;
    }

    public static bool IsLessOrEqualThan<T>(this T first, T second)
        where T : IComparable<T>
    {
        return first.CompareTo(second) <= 0;
    }
}