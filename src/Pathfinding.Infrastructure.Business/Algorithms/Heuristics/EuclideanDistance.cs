using Pathfinding.Service.Interface;
using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics;

public sealed class EuclideanDistance : Distance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override double Calculate(IPathfindingVertex first,
        IPathfindingVertex second)
    {
        var result = base.Calculate(first, second);
        return Math.Round(Math.Sqrt(result), digits: 4);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Zip(int first, int second)
    {
        return Math.Pow(first - second, 2);
    }
}