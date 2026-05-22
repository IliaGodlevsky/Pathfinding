using Pathfinding.Domain.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.Presentation.Console.Factories;

public interface IStepRuleFactory
{
    IReadOnlyCollection<StepRules> AvailableStepRules { get; }

    IStepRule Create(StepRules stepRule);
}