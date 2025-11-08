using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.StepRules;

[Category("Unit")]
public class LandscapeStepRuleTests
{
    [Test]
    public void CalculateStepCost_ReturnsAbsoluteDifference()
    {
        var current = new TestPathfindingVertex(new Coordinate(0, 0), cost: 8);
        var neighbour = new TestPathfindingVertex(new Coordinate(1, 0), cost: 3);

        var rule = new LandscapeStepRule();
        var cost = rule.CalculateStepCost(neighbour, current);

        Assert.That(cost, Is.EqualTo(5));
    }
}
