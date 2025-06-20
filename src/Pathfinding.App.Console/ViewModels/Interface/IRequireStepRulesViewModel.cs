﻿using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireStepRuleViewModel
{
    IReadOnlyCollection<StepRules> AllowedStepRules { get; }

    StepRules? StepRule { get; set; }
}