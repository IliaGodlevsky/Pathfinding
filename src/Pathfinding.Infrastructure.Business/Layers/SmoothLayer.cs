using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Extensions;

namespace Pathfinding.Infrastructure.Business.Layers;

public class SmoothLayer(int level) : ILayer
{
    public void Overlay(IGraph<IVertex> graph)
    {
        int lvl = level;
        while (lvl-- > 0)
        {
            var costs = graph.Select(GetAverageCost);
            foreach (var (vertex, price) in graph.Zip(costs, (v, p) => (Vertex: v, Price: p)))
            {
                var range = vertex.Cost.CostRange;
                var cost = range.ReturnInRange(price);
                vertex.Cost.CurrentCost = cost;
            }
        }
    }

    private static int GetAverageCost(IVertex vertex)
    {
        return (int)vertex.Neighbors
            .Average(neighbour => CalculateMeanCost(neighbour, vertex));
    }

    private static double CalculateMeanCost(IVertex neighbor, IVertex vertex)
    {
        var neighbourCost = neighbor.Cost.CurrentCost;
        var vertexCost = vertex.Cost.CurrentCost;
        var averageCost = ((double)vertexCost + neighbourCost) / 2;
        var roundAverageCost = Math.Ceiling(averageCost);
        return Convert.ToInt32(roundAverageCost);
    }
}