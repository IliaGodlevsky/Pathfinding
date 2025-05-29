using Pathfinding.Domain.Interface;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class Layers(params ILayer[] layers) : List<ILayer>(layers), ILayer
{
    public static readonly Layers Empty = [];

    public Layers(IEnumerable<ILayer> layers)
        : this([.. layers])
    {

    }

    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var layer in layers)
        {
            layer.Overlay(graph);
        }
    }
}
