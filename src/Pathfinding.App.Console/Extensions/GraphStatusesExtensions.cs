using Pathfinding.App.Console.Resources;
using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.Extensions
{
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
}
