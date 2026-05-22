using Pathfinding.Domain.Enums;
using Pathfinding.Presentation.Console.Resources;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class RunStatusesExtensions
{
    public static string ToStringRepresentation(this RunStatuses statuses)
    {
        return statuses switch
        {
            RunStatuses.Success => Resource.Success,
            RunStatuses.Failure => Resource.Failure,
            _ => string.Empty,
        };
    }
}
