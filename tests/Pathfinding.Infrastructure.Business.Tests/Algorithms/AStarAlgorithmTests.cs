using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class AStarAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_RespectsHeuristicAndCost()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new AStarAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: TestGraphFactory.ExpectedPathLength, expectedCost: TestGraphFactory.GetExpectedPathCost());
    }
}
