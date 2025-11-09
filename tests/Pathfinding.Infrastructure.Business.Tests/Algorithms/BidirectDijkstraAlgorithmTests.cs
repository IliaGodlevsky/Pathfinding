using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class BidirectDijkstraAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_FindsShortestIntersection()
    {
        var graph = TestGraphFactory.CreateLinearGraph();
        var algorithm = new BidirectDijkstraAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 18, expectedCost: TestGraphFactory.GetLinearPathCost());
    }
}
