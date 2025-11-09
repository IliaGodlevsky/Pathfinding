using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class DepthFirstAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_TraversesToTarget()
    {
        var graph = TestGraphFactory.CreateBranchingGraph();
        var algorithm = new DepthFirstAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 18, expectedCost: TestGraphFactory.GetLinearPathCost());
    }
}
