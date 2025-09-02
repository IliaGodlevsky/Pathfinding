using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class LayerExtensions
{
    public static async ValueTask OverlayAsync<T>(this ILayer layer,
        IGraph<T> graph, CancellationToken token = default)
        where T : IVertex
    {
        await Task.Run(() => layer.Overlay((IGraph<IVertex>)graph), token).ConfigureAwait(false);
    }
}