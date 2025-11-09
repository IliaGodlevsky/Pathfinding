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
        var linearPath = TestGraphFactory.GetLinearPathCoordinates().ToList();
        var verticesByCoordinate = graph.Vertices.ToDictionary(vertex => vertex.Position);
        var traces = new Dictionary<Coordinate, IPathfindingVertex>();

        for (int index = linearPath.Count - 1; index > 0; index--)
        {
            var current = linearPath[index];
            var parent = linearPath[index - 1];
            traces[current] = verticesByCoordinate[parent];
        }

        var path = new GraphPath(traces, graph.Target);

        var expectedCount = linearPath.Count - 1;
        Assert.That(path, Has.Count.EqualTo(expectedCount));
        Assert.That(path.Cost, Is.EqualTo(expectedCount));

        var sequence = path.ToList();
        var expectedSequence = linearPath
            .AsEnumerable()
            .Reverse()
            .Take(expectedCount)
            .ToArray();
        Assert.That(sequence, Is.EqualTo(expectedSequence));
    }
}
