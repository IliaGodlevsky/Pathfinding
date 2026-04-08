using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class MazeLayer : ILayer
{
    private static readonly (int Dx, int Dy)[] Directions =
    [
        (0, -1),
        (1, 0),
        (0, 1),
        (-1, 0)
    ];

    public void Overlay(IGraph<IVertex> graph)
    {
        int width = graph.DimensionsSizes.ElementAtOrDefault(0);
        int length = graph.DimensionsSizes.ElementAtOrDefault(1);
        if (width <= 0 || length <= 0)
        {
            return;
        }

        foreach (var vertex in graph)
        {
            vertex.IsObstacle = false;
            vertex.Neighbors = [];
        }

        var seed = CreateSeed(graph, width, length);
        var random = new Random(seed);
        int startX = random.Next(0, width);
        int startY = random.Next(0, length);
        var visited = new HashSet<Coordinate>();
        var edges = new Dictionary<Coordinate, HashSet<Coordinate>>();
        var stack = new Stack<Coordinate>();
        var start = new Coordinate([startX, startY]);

        foreach (var vertex in graph)
        {
            edges[vertex.Position] = [];
        }

        stack.Push(start);
        visited.Add(start);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var nextCandidates = Directions
                .OrderBy(_ => random.Next())
                .Select(x => Next(current, x.Dx, x.Dy))
                .Where(IsInside)
                .Where(x => !visited.Contains(x))
                .ToArray();

            if (nextCandidates.Length == 0)
            {
                stack.Pop();
                continue;
            }

            var next = nextCandidates[0];
            edges[current].Add(next);
            edges[next].Add(current);
            visited.Add(next);
            stack.Push(next);
        }

        foreach (var (coordinate, neighbours) in edges)
        {
            graph.Get(coordinate).Neighbors = [.. neighbours.Select(graph.Get)];
        }

        Coordinate Next(Coordinate coordinate, int dx, int dy)
        {
            return new([coordinate.ElementAtOrDefault(0) + dx, coordinate.ElementAtOrDefault(1) + dy]);
        }

        bool IsInside(Coordinate coordinate)
        {
            int x = coordinate.ElementAtOrDefault(0);
            int y = coordinate.ElementAtOrDefault(1);
            return x >= 0 && y >= 0 && x < width && y < length;
        }
    }

    private static int CreateSeed(IGraph<IVertex> graph, int width, int length)
    {
        var hash = new HashCode();
        hash.Add(width);
        hash.Add(length);
        foreach (var vertex in graph)
        {
            hash.Add(vertex.Cost.CurrentCost);
            hash.Add(vertex.Position.ElementAtOrDefault(0));
            hash.Add(vertex.Position.ElementAtOrDefault(1));
        }
        return hash.ToHashCode();
    }

}
