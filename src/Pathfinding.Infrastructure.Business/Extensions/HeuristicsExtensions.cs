using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class HeuristicsExtensions
{
    public static WeightedHeuristic WithWeight(this IHeuristic heuristic, double? weight)
    {
        return new (heuristic, weight ?? 1);
    }
}