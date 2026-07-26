using Pathfinding.Data;
using Pathfinding.Service.Layers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Tests.Layers;

[Category("Unit")]
public sealed class ObstacleLayerTests
{
    [Test]
    public void Overlay_WithSameSeed_CreatesSameObstacleLayout()
    {
        var first = CreateGraph();
        var second = CreateGraph();

        new ObstacleLayer(35, new Random(12345)).Overlay(first);
        new ObstacleLayer(35, new Random(12345)).Overlay(second);

        Assert.That(
            second.Select(vertex => vertex.IsObstacle),
            Is.EqualTo(first.Select(vertex => vertex.IsObstacle)));
        Assert.That(first.Count(vertex => vertex.IsObstacle), Is.EqualTo(35));
    }

    private static Graph<FakeVertex> CreateGraph()
    {
        var vertices = Enumerable.Range(0, 100)
            .Select(index => new FakeVertex
            {
                Position = new Coordinate(index % 10, index / 10)
            })
            .ToArray();

        return new Graph<FakeVertex>(vertices, 10, 10);
    }
}
