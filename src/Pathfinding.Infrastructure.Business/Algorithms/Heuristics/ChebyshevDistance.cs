using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics;

public sealed class ChebyshevDistance : Distance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Aggregate(double a, double b)
    {
        return Math.Max(a, b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Zip(int first, int second)
    {
        return Math.Abs(first - second);
    }
}