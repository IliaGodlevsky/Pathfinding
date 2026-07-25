using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Layers;

public sealed class MazeObstacleLayer(Random random) : ILayer
{
    private static readonly (int X, int Y)[] Directions =
    [
        (2, 0),
        (-2, 0),
        (0, 2),
        (0, -2)
    ];

    public void Overlay(IGraph<IVertex> graph)
    {
        foreach (var vertex in graph)
        {
            vertex.IsObstacle = true;
        }

        if (graph.DimensionsSizes.Count < 2 ||
            graph.DimensionsSizes[0] < 3 ||
            graph.DimensionsSizes[1] < 3)
        {
            foreach (var vertex in graph)
            {
                vertex.IsObstacle = false;
            }
            return;
        }

        var width = graph.DimensionsSizes[0];
        var height = graph.DimensionsSizes[1];
        var start = new Coordinate(1, 1);
        var visited = new HashSet<Coordinate> { start };
        var pending = new Stack<Coordinate>();
        pending.Push(start);
        graph.Get(start).IsObstacle = false;

        while (pending.TryPeek(out var current))
        {
            var candidates = Directions
                .Select(direction => new Coordinate(
                    current[0] + direction.X,
                    current[1] + direction.Y))
                .Where(position => position[0] > 0 && position[0] < width - 1 &&
                    position[1] > 0 && position[1] < height - 1 &&
                    !visited.Contains(position))
                .OrderBy(_ => random.Next())
                .ToArray();

            if (candidates.Length == 0)
            {
                pending.Pop();
                continue;
            }

            var next = candidates[0];
            var passage = new Coordinate(
                (current[0] + next[0]) / 2,
                (current[1] + next[1]) / 2);
            graph.Get(passage).IsObstacle = false;
            graph.Get(next).IsObstacle = false;
            visited.Add(next);
            pending.Push(next);
        }
    }
}
