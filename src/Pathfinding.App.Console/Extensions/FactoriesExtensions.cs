using Pathfinding.App.Console.Factories;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.App.Console.Extensions;

internal static class FactoriesExtensions
{
    public static IHeuristic CreateHeuristic(this IHeuristicsFactory factory, IAlgorithmBuildInfo info)
    {
        return factory.CreateHeuristic(info.Heuristics!.Value, info.Weight!.Value);
    }
}