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
            vertex.IsObstacle = true;
        }

        if (width < 3 || length < 3)
        {
            foreach (var vertex in graph)
            {
                vertex.IsObstacle = false;
            }
            return;
        }

        int startX = RandomOddWithin(width);
        int startY = RandomOddWithin(length);
        var visited = new HashSet<Coordinate>();
        var stack = new Stack<Coordinate>();
        var start = new Coordinate([startX, startY]);

        stack.Push(start);
        visited.Add(start);
        Carve(start);

        while (stack.Count > 0)
        {
            var current = stack.Peek();
            var nextCandidates = Directions
                .OrderBy(_ => Random.Shared.Next())
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
            Carve(wall);
            Carve(next);
            visited.Add(next);
            stack.Push(next);
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

        void Carve(Coordinate coordinate)
        {
            graph.Get(coordinate).IsObstacle = false;
        }
    }

    private static int RandomOddWithin(int max)
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
        return Random.Shared.Next(0, count) * 2 + 1;
    }

    private static bool IsOdd(int number) => number % 2 != 0;
}
