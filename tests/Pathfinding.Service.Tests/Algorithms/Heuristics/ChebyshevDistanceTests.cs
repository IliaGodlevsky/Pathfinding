using Pathfinding.Service.Algorithms.Heuristics;
using Pathfinding.Service.Tests.Algorithms.Helpers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Service.Tests.Algorithms.Heuristics;

[Category("Unit")]
public class ChebyshevDistanceTests
{
    [Test]
    public void Calculate_ReturnsMaximumAxisDifference()
    {
        var first = new TestPathfindingVertex(new Coordinate(0, 0));
        var second = new TestPathfindingVertex(new Coordinate(3, 5));

        var heuristic = new ChebyshevDistance();
        var value = heuristic.Calculate(first, second);

        Assert.That(value, Is.EqualTo(5));
    }
}
