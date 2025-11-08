using Pathfinding.Infrastructure.Business.Algorithms.Heuristics;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Heuristics;

[Category("Unit")]
public class EuclideanDistanceTests
{
    [Test]
    public void Calculate_ReturnsRoundedEuclideanDistance()
    {
        var first = new TestPathfindingVertex(new Coordinate(0, 0));
        var second = new TestPathfindingVertex(new Coordinate(3, 5));

        var heuristic = new EuclideanDistance();
        var value = heuristic.Calculate(first, second);

        Assert.That(value, Is.EqualTo(5.8310));
    }
}
