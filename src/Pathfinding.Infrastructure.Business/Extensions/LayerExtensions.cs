using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class LayerExtensions
{
    public static async ValueTask OverlayAsync<T>(this ILayer layer, IGraph<T> graph)
        where T : IVertex
    {
        await Task.Run(() => layer.Overlay((IGraph<IVertex>)graph)).ConfigureAwait(false);
    }
}