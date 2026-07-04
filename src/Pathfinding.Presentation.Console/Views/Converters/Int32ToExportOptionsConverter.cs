using Pathfinding.Presentation.Console.Models;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Pathfinding.Presentation.Console.Views.Converters;

internal sealed class Int32ToExportOptionsConverter : BindingTypeConverter<int, ExportOptions>
{
    public override int GetAffinityForObjects()
    {
        return 100;
    }

    public override bool TryConvert(int from, object conversionHint, [MaybeNullWhen(true)] out ExportOptions result)
    {
        return Enum.TryParse(from.ToString(), out result);
    }
}
