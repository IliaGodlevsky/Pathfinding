using Pathfinding.Domain.Interface;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Layers;

public sealed class MazeObstacleLayer(int obstaclePercent, Random random) : ILayer
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
            AdjustObstacleCount(graph);
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

        AdjustObstacleCount(graph);
    }

    private void AdjustObstacleCount(IGraph<IVertex> graph)
    {
        var target = graph.Count * obstaclePercent / 100;
        var current = graph.Count(vertex => vertex.IsObstacle);
        if (current > target)
        {
            foreach (var vertex in graph
                .Where(vertex => vertex.IsObstacle)
                .OrderBy(_ => random.Next())
                .Take(current - target))
            {
                vertex.IsObstacle = false;
            }
            return;
        }

        while (current < target)
        {
            var removed = false;
            foreach (var vertex in graph
                .Where(vertex => !vertex.IsObstacle)
                .OrderBy(_ => random.Next()))
            {
                vertex.IsObstacle = true;
                if (IsOpenAreaConnected(graph))
                {
                    current++;
                    removed = true;
                    break;
                }
                vertex.IsObstacle = false;
            }
            if (!removed)
            {
                break;
            }
        }
    }

    private static bool IsOpenAreaConnected(IGraph<IVertex> graph)
    {
        var open = graph.Where(vertex => !vertex.IsObstacle).ToArray();
        if (open.Length < 2)
        {
            return true;
        }

        var visited = new HashSet<Coordinate> { open[0].Position };
        var pending = new Queue<Coordinate>();
        pending.Enqueue(open[0].Position);
        while (pending.TryDequeue(out var current))
        {
            foreach (var (X, Y) in Directions)
            {
                var next = new Coordinate(
                    current[0] + X / 2,
                    current[1] + Y / 2);
                if (next[0] < 0 || next[0] >= graph.DimensionsSizes[0] ||
                    next[1] < 0 || next[1] >= graph.DimensionsSizes[1] ||
                    graph.Get(next).IsObstacle || !visited.Add(next))
                {
                    continue;
                }
                pending.Enqueue(next);
            }
        }
        return visited.Count == open.Length;
    }
}
