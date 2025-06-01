using Pathfinding.Service.Interface;
using Pathfinding.Shared.Extensions;
using System.Runtime.CompilerServices;

namespace Pathfinding.Infrastructure.Business.Algorithms.Heuristics;

public abstract class Distance : IHeuristic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual double Calculate(
        IPathfindingVertex first,
        IPathfindingVertex second)
    {
        return first.Position
            .Zip(second.Position, Zip)
            .AggregateOrDefault(Aggregate);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual double Aggregate(double a, double b) => a + b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected abstract double Zip(int first, int second);
}