using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Extensions;

using static System.Linq.Enumerable;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class ObstacleLayer(int obstaclePercent) : ILayer
{
    public void Overlay(IGraph<IVertex> graph)
    {
        var obstaclesCount = graph.Count * obstaclePercent / 100;
        var regularsCount = graph.Count - obstaclesCount;
        Repeat(true, obstaclesCount)
           .Concat(Repeat(false, regularsCount))
           .OrderBy(_ => Random.Shared.NextDouble()) // shuffle
           .Zip(graph, (o, v) => (Vertex: v, Obstacle: o))
           .ForEach(item => item.Vertex.IsObstacle = item.Obstacle);
    }
}
