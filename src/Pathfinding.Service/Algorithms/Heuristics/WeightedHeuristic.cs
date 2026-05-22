using Pathfinding.Service.Interface;
using System.Runtime.CompilerServices;

namespace Pathfinding.Service.Algorithms.Heuristics;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public sealed class WeightedHeuristic(IHeuristic heuristic, double weight) : IHeuristic
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public double Calculate(IPathfindingVertex first, IPathfindingVertex second)
    {
        return heuristic.Calculate(first, second) * weight;
    }
}
