using System.Collections.Generic;
using System.Linq;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.GraphPaths;

[Category("Unit")]
public class GraphPathTests
{
    [Test]
    public void GraphPath_BuildsSequenceFromTraces()
    {
        var graph = TestGraphFactory.CreateLinearGraph();
        var middle = graph.Vertices.Single(v => v.Position.Equals(new Coordinate(1, 0)));
        var traces = new Dictionary<Coordinate, IPathfindingVertex>
        {
            [graph.Target.Position] = middle,
            [middle.Position] = graph.Start,
        };

        var path = new GraphPath(traces, graph.Target);

        Assert.That(path.Count, Is.EqualTo(2));
        Assert.That(path.Cost, Is.EqualTo(2));

        var sequence = path.ToList();
        Assert.That(sequence, Is.EqualTo(new[]
        {
            graph.Target.Position,
            middle.Position,
        }));
    }
}
