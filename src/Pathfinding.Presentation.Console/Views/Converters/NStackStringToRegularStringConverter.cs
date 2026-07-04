using NStack;
using ReactiveUI;

namespace Pathfinding.Presentation.Console.Views.Converters;

internal sealed class NStackStringToRegularStringConverter : BindingTypeConverter<ustring, string>
{
    public override int GetAffinityForObjects()
    {
        return 100;
    }

    public override bool TryConvert(
        ustring from,
        object conversionHint,
        out string result)
    {
        result = from?.ToString() ?? string.Empty;
        return true;
    }
}
