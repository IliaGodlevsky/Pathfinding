using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class CostGreedyAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_UsesStepCostAsHeuristic()
    {
        var graph = TestGraphFactory.CreateBranchingGraph();
        var algorithm = new CostGreedyAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 18, expectedCost: TestGraphFactory.GetLinearPathCost());
    }
}
