using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.StepRules;

[Category("Unit")]
public class DefaultStepRuleTests
{
    [Test]
    public void CalculateStepCost_ReturnsNeighbourCost()
    {
        var current = new TestPathfindingVertex(new Coordinate(0, 0), cost: 5);
        var neighbour = new TestPathfindingVertex(new Coordinate(1, 0), cost: 3);

        var rule = new DefaultStepRule();
        var cost = rule.CalculateStepCost(neighbour, current);

        Assert.That(cost, Is.EqualTo(3));
    }
}
