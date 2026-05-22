using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Resources;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class HeuristicsExtensions
{
    public static string ToStringRepresentation(this Heuristics heuristics)
    {
        return heuristics switch
        {
            Heuristics.Euclidean => Resource.Euclidian,
            Heuristics.Chebyshev => Resource.Chebyshev,
            Heuristics.Diagonal => Resource.Diagonal,
            Heuristics.Manhattan => Resource.Manhattan,
            _ => string.Empty
        };
    }
}
