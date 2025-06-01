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

    protected abstract IReadOnlyCollection<Coordinate> CreateNeighborhood(Coordinate coordinate);

    private static List<IVertex> GetNeighboursWithinGraph(
        IReadOnlyCollection<Coordinate> self,
        IGraph<IVertex> graph)
    {
        return [.. self.Where(IsInRange).Distinct().Select(graph.Get)];

        bool IsInRange(Coordinate coordinate)
        {
            return coordinate
                .Zip(graph.DimensionsSizes, (x, y) => (Position: x, Dimension: y))
                .All(z => z.Position < z.Dimension && z.Position >= 0);
        }
    }
}
