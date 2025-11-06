using Pathfinding.Service.Interface;
using Pathfinding.Shared.Extensions;
using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics;

public sealed class CanberraDistance : Distance
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override double Calculate(IPathfindingVertex first, IPathfindingVertex second)
    {
        return first.Position
            .Zip(second.Position, Zip)
            .AggregateOrDefault(Aggregate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected override double Zip(int first, int second)
    {
        var numerator = Math.Abs(first - second);
        var denominator = Math.Abs(first) + Math.Abs(second);
        return denominator == 0 ? 0 : numerator / denominator;
    }
}
