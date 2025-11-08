using System.Collections.Generic;
using System.Linq;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal static class AlgorithmAssert
{
    public static IReadOnlyList<Coordinate> Enumerate(IGraphPath path)
    {
        return path.ToList();
    }

    public static void PathHasExpectedMetrics(
        IGraphPath path,
        TestGraph graph,
        int expectedLength,
        double expectedCost,
        double tolerance = 1e-6)
    {
        Assert.That(path.Count, Is.EqualTo(expectedLength));
        Assert.That(path.Cost, Is.EqualTo(expectedCost).Within(tolerance));

        var coordinates = Enumerate(path);
        Assert.That(coordinates.Count, Is.EqualTo(expectedLength));
        Assert.That(coordinates[0], Is.EqualTo(graph.Target.Position));
        Assert.That(coordinates, Does.Contain(graph.Target.Position));
    }
}
