using Pathfinding.Domain.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class MazeLayer : ILayer
{
    private static readonly (int Dx, int Dy)[] Directions =
    [
        (0, -2),
        (2, 0),
        (0, 2),
        (-2, 0)
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

        if (width < 3 || length < 3)
        {
            ConnectDense(graph, width, length);
            return;
        }

        var seed = CreateSeed(graph, width, length);
        var random = new Random(seed);
        int startX = RandomOddWithin(width, random);
        int startY = RandomOddWithin(length, random);
        var visited = new HashSet<Coordinate>();
        var carved = new HashSet<Coordinate>();
        var stack = new Stack<Coordinate>();
        var start = new Coordinate([startX, startY]);

        stack.Push(start);
        visited.Add(start);
        carved.Add(start);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var nextCandidates = Directions
                .OrderBy(_ => random.Next())
                .Select(x => Next(current, x.Dx, x.Dy))
                .Where(IsInside)
                .Where(x => IsOdd(x.ElementAtOrDefault(0)) && IsOdd(x.ElementAtOrDefault(1)))
                .Where(x => !visited.Contains(x))
                .ToArray();

            if (nextCandidates.Length == 0)
            {
                stack.Pop();
                continue;
            }

            var next = nextCandidates[0];
            var wall = Between(current, next);
            carved.Add(wall);
            carved.Add(next);
            visited.Add(next);
            stack.Push(next);
        }

        foreach (var coordinate in carved)
        {
            graph.Get(coordinate).Neighbors = [.. GetNeighbors(coordinate)];
        }

        IEnumerable<IVertex> GetNeighbors(Coordinate coordinate)
        {
            var candidates = new[]
            {
                new Coordinate([coordinate.ElementAtOrDefault(0), coordinate.ElementAtOrDefault(1) - 1]),
                new Coordinate([coordinate.ElementAtOrDefault(0) + 1, coordinate.ElementAtOrDefault(1)]),
                new Coordinate([coordinate.ElementAtOrDefault(0), coordinate.ElementAtOrDefault(1) + 1]),
                new Coordinate([coordinate.ElementAtOrDefault(0) - 1, coordinate.ElementAtOrDefault(1)])
            };
            foreach (var candidate in candidates)
            {
                if (IsInside(candidate) && carved.Contains(candidate))
                {
                    yield return graph.Get(candidate);
                }
            }
        }

        Coordinate Next(Coordinate coordinate, int dx, int dy)
        {
            return new([coordinate.ElementAtOrDefault(0) + dx, coordinate.ElementAtOrDefault(1) + dy]);
        }

        Coordinate Between(Coordinate from, Coordinate to)
        {
            int x = (from.ElementAtOrDefault(0) + to.ElementAtOrDefault(0)) / 2;
            int y = (from.ElementAtOrDefault(1) + to.ElementAtOrDefault(1)) / 2;
            return new([x, y]);
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

    private static void ConnectDense(IGraph<IVertex> graph, int width, int length)
    {
        foreach (var vertex in graph)
        {
            int x = vertex.Position.ElementAtOrDefault(0);
            int y = vertex.Position.ElementAtOrDefault(1);
            var coords = new[]
            {
                new Coordinate([x, y - 1]),
                new Coordinate([x + 1, y]),
                new Coordinate([x, y + 1]),
                new Coordinate([x - 1, y])
            };
            vertex.Neighbors = [.. coords
                .Where(c => c.ElementAtOrDefault(0) >= 0
                            && c.ElementAtOrDefault(1) >= 0
                            && c.ElementAtOrDefault(0) < width
                            && c.ElementAtOrDefault(1) < length)
                .Select(graph.Get)];
        }
    }

    private static int RandomOddWithin(int max, Random random)
    {
        int upper = max - 1;
        if (upper % 2 == 0)
        {
            upper--;
        }

        if (upper <= 1)
        {
            return 1;
        }

        int count = (upper + 1) / 2;
        return random.Next(0, count) * 2 + 1;
    }

    private static bool IsOdd(int number) => number % 2 != 0;
}
