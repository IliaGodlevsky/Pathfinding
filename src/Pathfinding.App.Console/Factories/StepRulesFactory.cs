using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface;

namespace Pathfinding.App.Console.Factories;

public sealed class StepRulesFactory : IStepRuleFactory
{
    private readonly Dictionary<StepRules, IStepRule> stepRules;

    public StepRulesFactory(IEnumerable<Meta<IStepRule>> stepRules)
    {
        this.stepRules = stepRules.ToDictionary(x => (StepRules)x.Metadata[MetadataKeys.StepRule], x => x.Value);
    }

    public IStepRule CreateStepRule(StepRules stepRule)
    {
        if (stepRules.TryGetValue(stepRule, out var value))
        {
            return value;
        }

        throw new KeyNotFoundException($"{stepRule} was not found");
    }
}