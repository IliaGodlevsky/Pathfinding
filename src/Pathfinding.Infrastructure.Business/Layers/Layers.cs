using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class Layers(params ILayer[] layers) : List<ILayer>(layers), ILayer
{
    public Layers(IEnumerable<ILayer> layers)
        : this(layers.ToArray())
    {

    }

    public void Overlay(IGraph<IVertex> graph)
    {
        ForEach(layer => layer.Overlay(graph));
    }
}
