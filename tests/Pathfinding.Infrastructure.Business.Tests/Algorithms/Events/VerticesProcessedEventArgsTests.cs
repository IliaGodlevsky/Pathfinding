using Pathfinding.Infrastructure.Business.Algorithms.Events;
using Pathfinding.Infrastructure.Business.Tests.Algorithms.Helpers;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Events;

[Category("Unit")]
public class VerticesProcessedEventArgsTests
{
    [Test]
    public void Constructor_StoresCurrentAndEnqueuedCoordinates()
    {
        var current = new TestPathfindingVertex(new Coordinate(0, 0));
        var first = new TestPathfindingVertex(new Coordinate(1, 0));
        var second = new TestPathfindingVertex(new Coordinate(2, 0));

        var args = new VerticesProcessedEventArgs(current, new[] { first, second });

        Assert.That(args.Current, Is.EqualTo(current.Position));
        Assert.That(args.Enqueued, Is.EqualTo(new[]
        {
            first.Position,
            second.Position
        }));
    }
}
