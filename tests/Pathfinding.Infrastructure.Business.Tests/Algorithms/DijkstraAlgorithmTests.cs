using Pathfinding.Infrastructure.Business.Algorithms;
using Pathfinding.Infrastructure.Business.Algorithms.StepRules;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms;

[Category("Unit")]
public class DijkstraAlgorithmTests
{
    [Test]
    public void FindPath_WithBranchingGraph_ChoosesLowestCostRoute()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new DijkstraAlgorithm(graph.Range, new DefaultStepRule());

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 11, expectedCost: 61);
    }
}
