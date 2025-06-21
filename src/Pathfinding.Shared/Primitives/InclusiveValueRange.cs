using Pathfinding.Shared.Extensions;
using System.Runtime.CompilerServices;

namespace Pathfinding.Shared.Primitives;

/// <summary>
/// Represents an inclusive value range, value of which should 
/// implement <see cref="IComparable{T}"/>
/// </summary>
/// <typeparam name="T">The type of values, that 
/// will represent the range</typeparam>
public readonly record struct InclusiveValueRange<T>
    where T : IComparable<T>
{
    public T UpperValueOfRange { get; }

    public T LowerValueOfRange { get; }

    /// <param name="upperValueOfRange"></param>
    /// <param name="lowerValueOfRange"></param>
    /// <remarks>No matter which value is greater, 
    /// value are assigned correctly anyway</remarks>
    public InclusiveValueRange(T upperValueOfRange,
        T lowerValueOfRange = default)
    {
        if (upperValueOfRange.IsLessThan(lowerValueOfRange))
        {
            UpperValueOfRange = lowerValueOfRange;
            LowerValueOfRange = upperValueOfRange;
        }
        else
        {
            UpperValueOfRange = upperValueOfRange;
            LowerValueOfRange = lowerValueOfRange;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator (T UpperValueRange, T LowerValueRange)(InclusiveValueRange<T> range)
    {
        return (range.UpperValueOfRange, range.LowerValueOfRange);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator InclusiveValueRange<T>((T UpperValueRange, T LowerValueRange) range)
    {
        return new(range.UpperValueRange, range.LowerValueRange);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override string ToString()
    {
        return $"[{LowerValueOfRange},{UpperValueOfRange}]";
    }
}