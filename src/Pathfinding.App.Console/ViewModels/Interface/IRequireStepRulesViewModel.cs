using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireStepRuleViewModel
{
    IReadOnlyCollection<StepRules> AvailiableStepRules { get; }

    StepRules? StepRule { get; set; }
}