using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Algorithms.StepRules;
using Pathfinding.Service.Tests.Algorithms.Helpers;

namespace Pathfinding.Service.Tests.Algorithms;

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
