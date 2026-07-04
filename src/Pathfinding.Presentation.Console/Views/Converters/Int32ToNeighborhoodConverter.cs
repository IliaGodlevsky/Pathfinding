using Pathfinding.Domain.Enums;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;

namespace Pathfinding.Presentation.Console.Views.Converters;

internal sealed class Int32ToNeighborhoodsConverter : BindingTypeConverter<int, Neighborhoods>
{
    public override int GetAffinityForObjects()
    {
        return 100;
    }

    public override bool TryConvert(int from, object conversionHint, [MaybeNullWhen(true)] out Neighborhoods result)
    {
        return Enum.TryParse(from.ToString(), out result);
    }
}
