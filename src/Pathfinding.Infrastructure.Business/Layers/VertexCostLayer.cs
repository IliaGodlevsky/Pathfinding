using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class VertexCostLayer(InclusiveValueRange<int> costRange,
    Func<InclusiveValueRange<int>, IVertexCost> generator) : ILayer
{
    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var vertex in graph)
        {
            vertex.Cost = generator(costRange);
        }
    }
}
