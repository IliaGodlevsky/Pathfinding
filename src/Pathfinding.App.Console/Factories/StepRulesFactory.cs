using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class StepRulesFactory(IEnumerable<Meta<IStepRule>> stepRules) : IStepRuleFactory
{
    private readonly Dictionary<StepRules, IStepRule> stepRules = stepRules.ToDictionary(
            x => (StepRules)x.Metadata[MetadataKeys.StepRule],
            x => x.Value);

    public IReadOnlyCollection<StepRules> Allowed => stepRules.Keys;

    public IStepRule CreateStepRule(StepRules stepRule)
    {
        return stepRules.TryGetValue(stepRule, out var value) 
            ? value 
            : throw new KeyNotFoundException($"{stepRule} was not found");
    }
}