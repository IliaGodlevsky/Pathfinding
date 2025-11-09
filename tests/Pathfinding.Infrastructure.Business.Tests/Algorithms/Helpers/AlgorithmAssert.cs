using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal static class AlgorithmAssert
{
    public static void PathHasExpectedMetrics(
        IGraphPath path,
        TestGraph graph,
        int expectedLength,
        double expectedCost,
        double tolerance = 1e-6)
    {
        Assert.That(path, Has.Count.EqualTo(expectedLength));
        Assert.That(path.Cost, Is.EqualTo(expectedCost).Within(tolerance));

        IReadOnlyList<Coordinate> coordinates = [.. path];
        Assert.That(coordinates, Has.Count.EqualTo(expectedLength));
        Assert.Multiple(() =>
        {
            Assert.That(coordinates[0], Is.EqualTo(graph.Target.Position));
            Assert.That(coordinates, Does.Contain(graph.Target.Position));
        });
    }
}
