using System.Collections.Generic;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

internal static class TestGraphFactory
{
    public static TestGraph CreateLinearGraph()
    {
        var start = new TestPathfindingVertex(new Coordinate(0, 0));
        var middle = new TestPathfindingVertex(new Coordinate(1, 0));
        var target = new TestPathfindingVertex(new Coordinate(2, 0));

        start.ConnectBidirectional(middle);
        middle.ConnectBidirectional(target);

        return new TestGraph(
            start,
            target,
            new[] { start, middle, target });
    }

    public static TestGraph CreateBranchingGraph()
    {
        var start = new TestPathfindingVertex(new Coordinate(0, 0));
        var cheapMiddle = new TestPathfindingVertex(new Coordinate(1, 0), cost: 1);
        var expensiveMiddle = new TestPathfindingVertex(new Coordinate(0, 1), cost: 5);
        var target = new TestPathfindingVertex(new Coordinate(2, 0));

        start.ConnectBidirectional(cheapMiddle);
        start.ConnectBidirectional(expensiveMiddle);
        cheapMiddle.ConnectBidirectional(target);
        expensiveMiddle.ConnectBidirectional(target);

        return new TestGraph(
            start,
            target,
            new IPathfindingVertex[] { start, cheapMiddle, expensiveMiddle, target });
    }
}

internal sealed record TestGraph(
    IPathfindingVertex Start,
    IPathfindingVertex Target,
    IReadOnlyCollection<IPathfindingVertex> Vertices)
{
    public IReadOnlyCollection<IPathfindingVertex> Range => new[] { Start, Target };
}
