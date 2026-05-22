using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRequireStepRuleViewModel
{
    IReadOnlyCollection<StepRules> AvailiableStepRules { get; }

    StepRules? StepRule { get; set; }
}