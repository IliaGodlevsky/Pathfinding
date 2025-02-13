using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions
{
    internal static class SmoothLevelsExtensions
    {
        public static string ToStringRepresentation(this SmoothLevels level)
        {
            return level switch
            {
                SmoothLevels.No => Resource.No,
                SmoothLevels.Low => Resource.Low,
                SmoothLevels.Medium => Resource.Medium,
                SmoothLevels.High => Resource.High,
                SmoothLevels.Extreme => Resource.Extreme,
                _ => string.Empty,
            };
        }
    }
}
