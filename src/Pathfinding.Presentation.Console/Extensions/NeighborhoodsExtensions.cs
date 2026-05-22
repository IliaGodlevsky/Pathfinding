using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Resources;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class NeighborhoodsExtensions
{
    public static string ToStringRepresentation(this Neighborhoods neighborhood)
    {
        return neighborhood switch
        {
            Neighborhoods.Moore => Resource.Moore,
            Neighborhoods.VonNeumann => Resource.VonNeumann,
            Neighborhoods.Diagonal => Resource.DiagonalNeighborhood,
            Neighborhoods.Knight => Resource.Knight,
            _ => string.Empty
        };
    }
}
