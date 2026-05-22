using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Algorithms;
using Pathfinding.Service.Tests.Algorithms.Helpers;

namespace Pathfinding.Service.Tests.Algorithms;

[Category("Unit")]
public class BidirectAStarAlgorithmTests
{
    [Test]
    public void FindPath_WithLinearGraph_CombinesFrontiersAtMeetingPoint()
    {
        var graph = TestGraphFactory.CreateGraph();
        var algorithm = new BidirectAStarAlgorithm(graph.Range);

        var path = algorithm.FindPath();

        AlgorithmAssert.PathHasExpectedMetrics(path, graph, expectedLength: 12, expectedCost: 61);
    }
}
