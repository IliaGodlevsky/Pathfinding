using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Tests.Algorithms.Helpers;

namespace Pathfinding.Service.Tests.Algorithms;

[Category("Unit")]
public class AStarAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_RespectsHeuristicAndCost()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new AStarAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 11, expectedCost: 61);
    }
}
