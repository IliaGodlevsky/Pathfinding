using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Layers;

public sealed class VertexCostLayer(Func<InclusiveValueRange<int>, IVertexCost> generator) : ILayer
{
    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var vertex in graph)
        {
            vertex.Cost = generator(graph.CostRange);
        }
    }
}
