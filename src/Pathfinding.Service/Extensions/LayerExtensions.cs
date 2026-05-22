using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Extensions;

public static class LayerExtensions
{
    public static async ValueTask OverlayAsync<T>(this ILayer layer,
        IGraph<T> graph, CancellationToken token = default)
        where T : IVertex
    {
        await Task.Run(() => layer.Overlay((IGraph<IVertex>)graph), token).ConfigureAwait(false);
    }
}