using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Resources;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class GraphStatusesExtensions
{
    public static string ToStringRepresentation(this GraphStatuses status)
    {
        return status switch
        {
            GraphStatuses.Editable => Resource.Editable,
            GraphStatuses.Readonly => Resource.Readonly,
            _ => string.Empty
        };
    }
}
