using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions
{
    internal static class StepRulesExtensions
    {
        public static string ToStringRepresentation(this StepRules stepRules)
        {
            return stepRules switch
            {
                StepRules.Default => Resource.Default,
                StepRules.Landscape => Resource.Landscape,
                _ => string.Empty,
            };
        }
    }
}
