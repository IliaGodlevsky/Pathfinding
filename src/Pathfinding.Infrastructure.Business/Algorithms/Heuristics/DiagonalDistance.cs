using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics;

public sealed class DiagonalDistance : Distance
{
    private readonly double DiagonalCost = Math.Sqrt(2);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Aggregate(double a, double b)
    {
        return (DiagonalCost - 1) * Math.Min(a, b) + Math.Max(a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Zip(int first, int second)
    {
        return Math.Abs(first - second);
    }
}
