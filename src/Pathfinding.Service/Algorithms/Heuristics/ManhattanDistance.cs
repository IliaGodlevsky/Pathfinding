using System.Runtime.CompilerServices;

namespace Pathfinding.Service.Algorithms.Heuristics;

public sealed class ManhattanDistance : Distance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Zip(int first, int second)
    {
        return Math.Abs(first - second);
    }
}