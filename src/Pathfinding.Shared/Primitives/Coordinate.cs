using Pathfinding.Shared.Extensions;
using System.Collections;
using System.Runtime.CompilerServices;

namespace Pathfinding.Shared.Primitives;

public readonly record struct Coordinate : IReadOnlyList<int>
{
    public static readonly Coordinate Empty = new();

    private readonly string toString;
    private readonly int hashCode;

    private int[] CoordinatesValues { get; }

    public int Count => CoordinatesValues.Length;

    public int this[int index] => CoordinatesValues[index];

    public Coordinate(int numberOfDimensions, IReadOnlyList<int> coordinates)
    {
        CoordinatesValues = [.. coordinates.TakeOrDefault(numberOfDimensions)];
        toString = $"({string.Join(",", CoordinatesValues)})";
        hashCode = CoordinatesValues.AggregateOrDefault(HashCode.Combine);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate(IReadOnlyList<int> coordinates)
        : this(coordinates.Count, coordinates)
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate(IEnumerable<int> coordinates)
        : this([.. coordinates])
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Coordinate(params int[] coordinates)
        : this((IReadOnlyList<int>)coordinates)
    {

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Equals(Coordinate other)
    {
        return other.GetHashCode().Equals(GetHashCode());
    }

    public IEnumerator<int> GetEnumerator()
    {
        return CoordinatesValues.AsEnumerable().GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override int GetHashCode() => hashCode;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString() => toString;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}