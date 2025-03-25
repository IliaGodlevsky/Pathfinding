using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Layers;

public abstract class NeighborhoodLayer : ILayer
{
    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var vertex in graph)
        {
            var neighborhood = CreateNeighborhood(vertex.Position);
            var neighbours = GetNeighboursWithinGraph(neighborhood, graph);
            vertex.Neighbors = neighbours;
        }
    }

    protected abstract INeighborhood CreateNeighborhood(Coordinate coordinate);

    private List<IVertex> GetNeighboursWithinGraph(INeighborhood self,
        IGraph<IVertex> graph)
    {
        bool IsInRange(Coordinate coordinate)
        {
            return coordinate.CoordinatesValues
                .Zip(graph.DimensionsSizes, (x, y) => (Position: x, Dimension: y))
                .All(z => z.Position < z.Dimension && z.Position >= 0);
        }
        return self.Where(IsInRange).Distinct()
            .Select(graph.Get).ToList();
    }
}
