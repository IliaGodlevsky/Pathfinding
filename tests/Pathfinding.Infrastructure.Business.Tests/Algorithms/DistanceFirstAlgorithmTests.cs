using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class DistanceFirstAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_PrefersCloserVertex()
    {
        var graph = TestGraphFactory.CreateBranchingGraph();
        var algorithm = new DistanceFirstAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 2, expectedCost: 2);
    }
}
