using Pathfinding.Data;
using Pathfinding.Service.Layers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Tests.Layers;

[Category("Unit")]
public sealed class MazeObstacleLayerTests
{
    [Test]
    public void Overlay_WithSameSeed_CreatesConnectedReproducibleMaze()
    {
        var first = CreateGraph(11, 11);
        var second = CreateGraph(11, 11);

        new MazeObstacleLayer(60, new Random(12345)).Overlay(first);
        new MazeObstacleLayer(60, new Random(12345)).Overlay(second);

        var firstLayout = first.Select(vertex => vertex.IsObstacle).ToArray();
        var openVertices = first.Where(vertex => !vertex.IsObstacle).ToArray();
        Assert.That(second.Select(vertex => vertex.IsObstacle), Is.EqualTo(firstLayout));
        Assert.That(first.Count(vertex => vertex.IsObstacle), Is.EqualTo(72));
        Assert.That(CountReachable(first, openVertices[0].Position), Is.EqualTo(openVertices.Length));
    }

    [Test]
    public void Overlay_WhenGraphIsTooSmall_StillRespectsObstaclePercentage()
    {
        var graph = CreateGraph(2, 2);

        new MazeObstacleLayer(50, new Random(12345)).Overlay(graph);

        Assert.That(graph.Count(vertex => vertex.IsObstacle), Is.EqualTo(2));
    }

    [TestCase(20)]
    [TestCase(80)]
    public void Overlay_RespectsObstaclePercentage_AndKeepsOpenAreaConnected(
        int obstaclePercent)
    {
        var graph = CreateGraph(11, 11);

        new MazeObstacleLayer(obstaclePercent, new Random(12345)).Overlay(graph);

        var openVertices = graph.Where(vertex => !vertex.IsObstacle).ToArray();
        Assert.That(graph.Count(vertex => vertex.IsObstacle),
            Is.EqualTo(graph.Count * obstaclePercent / 100));
        Assert.That(CountReachable(graph, openVertices[0].Position),
            Is.EqualTo(openVertices.Length));
    }

    private static int CountReachable(Graph<FakeVertex> graph, Coordinate start)
    {
        var visited = new HashSet<Coordinate> { start };
        var pending = new Queue<Coordinate>();
        pending.Enqueue(start);
        var directions = new[] { (1, 0), (-1, 0), (0, 1), (0, -1) };
        while (pending.TryDequeue(out var current))
        {
            foreach (var direction in directions)
            {
                var next = new Coordinate(current[0] + direction.Item1,
                    current[1] + direction.Item2);
                if (next[0] < 0 || next[0] >= graph.DimensionsSizes[0] ||
                    next[1] < 0 || next[1] >= graph.DimensionsSizes[1] ||
                    graph.Get(next).IsObstacle || !visited.Add(next))
                {
                    continue;
                }
                pending.Enqueue(next);
            }
        }
        return visited.Count;
    }

    private static Graph<FakeVertex> CreateGraph(int width, int height)
    {
        var vertices = Enumerable.Range(0, width * height)
            .Select(index => new FakeVertex
            {
                Position = new Coordinate(index % width, index / width)
            })
            .ToArray();
        return new Graph<FakeVertex>(vertices, width, height);
    }
}
