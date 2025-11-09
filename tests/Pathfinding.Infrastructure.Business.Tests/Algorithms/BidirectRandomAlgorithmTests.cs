using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class BidirectRandomAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_DoesNotThrowAndReturnsPath()
    {
        var graph = TestGraphFactory.CreateLinearGraph();
        var algorithm = new BidirectRandomAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 18, expectedCost: 18);
    }
}
