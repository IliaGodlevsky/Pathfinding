using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.Infrastructure.Business.Extensions;

public static class LayerExtensions
{
    public static async ValueTask OverlayAsync<T>(this ILayer layer, IGraph<T> graph)
        where T : IVertex
    {
        await Task.Run(() => layer.Overlay((IGraph<IVertex>)graph)).ConfigureAwait(false);
    }

    public static ILayer ToNeighborhoodLayer(this Neighborhoods neighborhood)
    {
        return neighborhood switch
        {
            Neighborhoods.Moore => new MooreNeighborhoodLayer(),
            Neighborhoods.VonNeumann => new VonNeumannNeighborhoodLayer(),
            _ => Layers.Layers.Empty
        };
    }
}