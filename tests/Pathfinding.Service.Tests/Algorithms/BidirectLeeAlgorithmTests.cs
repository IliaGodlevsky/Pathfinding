using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Tests.Algorithms.Helpers;

namespace Pathfinding.Service.Tests.Algorithms;

[Category("Unit")]
public class BidirectLeeAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_UsesQueueFrontiers()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new BidirectLeeAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 10, expectedCost: 69);
    }
}
