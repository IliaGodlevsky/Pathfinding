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

    private static List<IVertex> GetNeighboursWithinGraph(INeighborhood self,
        IGraph<IVertex> graph)
    {
        return [.. self.OrderBy(x => x.ToString())
            .Where(IsInRange).Distinct().Select(graph.Get)];

        bool IsInRange(Coordinate coordinate)
        {
            return coordinate.CoordinatesValues
                .Zip(graph.DimensionsSizes, (x, y) => (Position: x, Dimension: y))
                .All(z => z.Position < z.Dimension && z.Position >= 0);
        }
    }
}
