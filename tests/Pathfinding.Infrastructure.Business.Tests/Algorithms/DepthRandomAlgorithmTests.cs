using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class DepthRandomAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_HandlesRandomSelection()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new DepthRandomAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: TestGraphFactory.ExpectedPathLength, expectedCost: TestGraphFactory.GetExpectedPathCost());
    }
}
