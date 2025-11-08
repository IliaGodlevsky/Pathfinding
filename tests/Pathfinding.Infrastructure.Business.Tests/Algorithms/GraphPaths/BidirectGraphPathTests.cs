using System.Collections.Generic;
using System.Linq;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.GraphPaths;

[Category("Unit")]
public class BidirectGraphPathTests
{
    [Test]
    public void BidirectGraphPath_CombinesForwardAndBackwardTraces()
    {
        var graph = TestGraphFactory.CreateLinearGraph();
        var middle = graph.Vertices.Single(v => v.Position.Equals(new Coordinate(1, 0)));

        var forwardTraces = new Dictionary<Coordinate, IPathfindingVertex>
        {
            [middle.Position] = graph.Start
        };
        var backwardTraces = new Dictionary<Coordinate, IPathfindingVertex>
        {
            [middle.Position] = graph.Target
        };

        var path = new BidirectGraphPath(forwardTraces, backwardTraces, middle);

        Assert.That(path.Count, Is.EqualTo(2));
        Assert.That(path.Cost, Is.EqualTo(2));

        var coordinates = path.ToList();
        Assert.That(coordinates, Is.EquivalentTo(new[]
        {
            graph.Target.Position,
            middle.Position
        }));
    }
}
