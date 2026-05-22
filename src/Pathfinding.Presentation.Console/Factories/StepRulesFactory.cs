using Autofac.Features.Metadata;
using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Service.Interface;

namespace Pathfinding.Presentation.Console.Factories;

public sealed class StepRulesFactory(IEnumerable<Meta<IStepRule>> stepRules) : IStepRuleFactory
{
    private readonly Dictionary<StepRules, IStepRule> stepRules = stepRules.ToDictionary(
            x => (StepRules)x.Metadata[MetadataKeys.StepRule],
            x => x.Value);

    public IReadOnlyCollection<StepRules> AvailableStepRules => stepRules.Keys;

    public IStepRule Create(StepRules stepRule)
    {
        return stepRules.GetValueOrDefault(stepRule)
            ?? throw new KeyNotFoundException($"{stepRule} was not found");
    }
}