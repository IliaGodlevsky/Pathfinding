using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions;

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
            Neighborhoods.Hexagonal => Resource.Hexagonal,
            Neighborhoods.ExtendedMoore => Resource.ExtendedMoore,
            _ => string.Empty
        };
    }
}
