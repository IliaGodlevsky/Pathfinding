using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions;

internal static class HeuristicsExtensions
{
    public static string ToStringRepresentation(this Heuristics heuristics)
    {
        return heuristics switch
        {
            Heuristics.Euclidian => Resource.Euclidian,
            Heuristics.Chebyshev => Resource.Chebyshev,
            Heuristics.Diagonal => Resource.Diagonal,
            Heuristics.Manhattan => Resource.Manhattan,
            _ => string.Empty
        };
    }
}
