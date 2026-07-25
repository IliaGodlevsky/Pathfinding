using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class GraphGeneratorsExtensions
{
    public static string ToStringRepresentation(this GraphGenerators generator)
    {
        return generator switch
        {
            GraphGenerators.RandomTerrain => "Random terrain",
            GraphGenerators.PerfectMaze => "Perfect maze",
            _ => throw new ArgumentOutOfRangeException(nameof(generator), generator, null)
        };
    }
}
