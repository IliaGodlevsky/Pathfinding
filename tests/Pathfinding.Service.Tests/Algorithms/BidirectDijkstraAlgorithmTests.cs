using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Tests.Algorithms.Helpers;

namespace Pathfinding.Service.Tests.Algorithms;

[Category("Unit")]
public class BidirectDijkstraAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_FindsShortestIntersection()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new BidirectDijkstraAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 10, expectedCost: 61);
    }
}
