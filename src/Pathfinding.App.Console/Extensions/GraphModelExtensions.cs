using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding;
using Pathfinding.Service.Interface.Models.Read;

namespace Pathfinding.App.Console.Extensions;

internal static class GraphModelExtensions
{
    public static Graph<T> CreateGraph<T>(this GraphModel<T> model)
        where T : IVertex
    {
        return new(model.Vertices, model.DimensionSizes);
    }
}
