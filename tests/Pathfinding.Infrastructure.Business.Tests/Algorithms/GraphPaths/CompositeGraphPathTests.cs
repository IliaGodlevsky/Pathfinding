using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;
using Pathfinding.Shared.Primitives;

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

        var composite = new CompositeGraphPath(new[] { first, second });

        Assert.That(composite.Count, Is.EqualTo(3));
        Assert.That(composite.Cost, Is.EqualTo(4));
        Assert.That(composite.ToList(), Is.EqualTo(new[]
        {
            new Coordinate(1, 0),
            new Coordinate(2, 0),
            new Coordinate(0, 0),
            new Coordinate(1, 0)
        }));
    }

    private sealed class FakeGraphPath : IGraphPath
    {
        private readonly IReadOnlyList<Coordinate> coordinates;

        public FakeGraphPath(IReadOnlyList<Coordinate> coordinates, int count, double cost)
        {
            this.coordinates = coordinates;
            Count = count;
            Cost = cost;
        }

        public double Cost { get; }

        public int Count { get; }

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
