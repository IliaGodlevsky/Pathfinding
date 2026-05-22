using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Resources;

namespace Pathfinding.Presentation.Console.Extensions;

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
