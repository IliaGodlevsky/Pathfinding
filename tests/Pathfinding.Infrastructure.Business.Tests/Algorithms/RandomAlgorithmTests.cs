using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class RandomAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_UsesRandomQueueSafely()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new RandomAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: TestGraphFactory.ExpectedPathLength, expectedCost: TestGraphFactory.GetExpectedPathCost());
    }
}
