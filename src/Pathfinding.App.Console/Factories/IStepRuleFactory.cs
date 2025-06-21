using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public interface IStepRuleFactory
{
    IReadOnlyCollection<StepRules> Allowed { get; }

    IStepRule CreateStepRule(StepRules stepRule);
}