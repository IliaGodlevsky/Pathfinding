using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics;

public sealed class HammingDistance : Distance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Zip(int first, int second)
    {
        return first == second ? 0 : 1;
    }
}
