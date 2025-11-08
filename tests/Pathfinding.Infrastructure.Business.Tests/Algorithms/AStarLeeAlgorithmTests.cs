using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class AStarLeeAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_ExpandsUsingHeuristicQueue()
    {
        var graph = TestGraphFactory.CreateBranchingGraph();
        var algorithm = new AStarLeeAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 2, expectedCost: 2);
    }
}
