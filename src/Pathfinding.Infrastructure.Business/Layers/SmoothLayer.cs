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
            foreach (var (Vertex, Price) in graph.Zip(costs, (v, p) => (Vertex: v, Price: p)))
            {
                var range = Vertex.Cost.CostRange;
                int cost = range.ReturnInRange(Price);
                Vertex.Cost.CurrentCost = cost;
            }
        }
    }

    private int GetAverageCost(IVertex vertex)
    {
        return (int)vertex.Neighbors
            .Average(neighbour => CalculateMeanCost(neighbour, vertex));
    }

    private static double CalculateMeanCost(IVertex neighbor, IVertex vertex)
    {
        int neighbourCost = neighbor.Cost.CurrentCost;
        int vertexCost = vertex.Cost.CurrentCost;
        double averageCost = (vertexCost + neighbourCost) / 2;
        double roundAverageCost = Math.Ceiling(averageCost);
        return Convert.ToInt32(roundAverageCost);
    }
}