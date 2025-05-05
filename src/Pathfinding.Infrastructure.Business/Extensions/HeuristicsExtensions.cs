using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class HeuristicsExtensions
{
    public static IHeuristic WithWeight(this IHeuristic heuristic, double? weight)
    {
        return new WeightedHeuristic(heuristic, weight ?? 1);
    }
}