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
        var graph = TestGraphFactory.CreateGraph();
        var linearPath = TestGraphFactory.GetExpectedPathCoordinates().ToList();
        var verticesByCoordinate = graph.Vertices.ToDictionary(vertex => vertex.Position);
        var intersectionIndex = linearPath.Count / 2;
        var intersection = verticesByCoordinate[linearPath[intersectionIndex]];

        var forwardTraces = new Dictionary<Coordinate, IPathfindingVertex>();
        for (int index = 1; index <= intersectionIndex; index++)
        {
            forwardTraces[linearPath[index]] = verticesByCoordinate[linearPath[index - 1]];
        }

        var backwardTraces = new Dictionary<Coordinate, IPathfindingVertex>();
        for (int index = linearPath.Count - 2; index >= intersectionIndex; index--)
        {
            backwardTraces[linearPath[index]] = verticesByCoordinate[linearPath[index + 1]];
        }

        var path = new BidirectGraphPath(forwardTraces, backwardTraces, intersection);

        var expectedCount = linearPath.Count - 1;
        var expectedCost = TestGraphFactory.GetExpectedPathCost();
        Assert.That(path, Has.Count.EqualTo(expectedCount));
        Assert.That(path.Cost, Is.EqualTo(expectedCost));

        var coordinates = path.ToList();
        var expectedCoordinates = linearPath
            .AsEnumerable()
            .Reverse()
            .Take(expectedCount);
        Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
    }
}
