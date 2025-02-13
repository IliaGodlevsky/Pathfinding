using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Layers;
using Pathfinding.Service.Interface.Models;

namespace Pathfinding.Infrastructure.Business.Extensions
{
    public static class GraphLayersInfoExtensions
    {
        public static ILayer ToLayers(this IGraphLayersInfo info)
        {
            var neighborhood = info.Neighborhood switch
            {
                Neighborhoods.Moore => new MooreNeighborhoodLayer(),
                Neighborhoods.VonNeumann => new VonNeumannNeighborhoodLayer(),
                _ => (ILayer)new Layers.Layers()
            };
            SmoothLayer smooth = info.SmoothLevel switch
            {
                SmoothLevels.Low => new(1),
                SmoothLevels.Medium => new(2),
                SmoothLevels.High => new(4),
                SmoothLevels.Extreme => new(7),
                _ => new(0)
            };
            return new Layers.Layers() { neighborhood, smooth };
        }
    }
}
