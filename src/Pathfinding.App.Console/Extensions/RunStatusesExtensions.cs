using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions
{
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
}
