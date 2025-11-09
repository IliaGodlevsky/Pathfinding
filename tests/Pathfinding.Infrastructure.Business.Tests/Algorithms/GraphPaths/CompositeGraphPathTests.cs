using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;
using System.Collections;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.GraphPaths;

[Category("Unit")]
public class CompositeGraphPathTests
{
    [Test]
    public void CompositeGraphPath_AggregatesCostAndCount()
    {
        var first = new FakeGraphPath([
            new Coordinate(2, 0),
            new Coordinate(1, 0)
        ], count: 2, cost: 3);
        var second = new FakeGraphPath([
            new Coordinate(1, 0),
            new Coordinate(0, 0)
        ], count: 1, cost: 1);

        var composite = new CompositeGraphPath([first, second]);

        Assert.That(composite, Has.Count.EqualTo(3));
        Assert.Multiple(() =>
        {
            Assert.That(composite.Cost, Is.EqualTo(4));
            Assert.That(composite.ToList(), Is.EqualTo(new[]
            {
            new Coordinate(1, 0),
            new Coordinate(2, 0),
            new Coordinate(0, 0),
            new Coordinate(1, 0)
        }));
        });
    }

    private sealed class FakeGraphPath(IReadOnlyList<Coordinate> coordinates, int count, double cost) : IGraphPath
    {
        public double Cost { get; } = cost;

        public int Count { get; } = count;

        public IEnumerator<Coordinate> GetEnumerator()
        {
            return coordinates.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
