using Pathfinding.Domain.Enums;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Pathfinding.Presentation.Console.Views.Converters;

internal sealed class Int32ToStepRulesConverter : BindingTypeConverter<int, StepRules?>
{
    public override int GetAffinityForObjects()
    {
        return 100;
    }

    public override bool TryConvert(int from, object conversionHint, [MaybeNullWhen(true)] out StepRules? result)
    {
        if (Enum.TryParse(from.ToString(), out StepRules o))
        {
            result = o;
            return true;
        }
        result = null;
        return false;
    }
}
