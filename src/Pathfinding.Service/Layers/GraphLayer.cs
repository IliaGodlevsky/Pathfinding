using Pathfinding.Data.Extensions;
using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;

namespace Pathfinding.Service.Layers;

public sealed class GraphLayer(IGraph<IVertex> layer) : ILayer
{
    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var vertex in layer)
        {
            var vert = graph.Get(vertex.Position);
            vert.IsObstacle = vertex.IsObstacle;
            vert.Cost = vertex.Cost.DeepClone();
            vert.Neighbors = [.. vertex.Neighbors
                .Select(x => x.Position)
                .Select(graph.Get)];
        }
    }
}
