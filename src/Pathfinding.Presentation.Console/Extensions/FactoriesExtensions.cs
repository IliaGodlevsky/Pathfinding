using Pathfinding.Presentation.Console.Factories;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class FactoriesExtensions
{
    public static IHeuristic CreateHeuristic(this IHeuristicsFactory factory, IAlgorithmBuildInfo info)
    {
        return factory.Create(info.Heuristics!.Value, info.Weight!.Value);
    }
}