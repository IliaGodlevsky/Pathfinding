using System.Linq;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.GraphPaths;

[Category("Unit")]
public class NullGraphPathTests
{
    [Test]
    public void NullGraphPath_HasZeroCostAndNoCoordinates()
    {
        var path = NullGraphPath.Instance;

        Assert.That(path.Count, Is.EqualTo(0));
        Assert.That(path.Cost, Is.EqualTo(0));
        Assert.That(path.ToList(), Is.Empty);
    }
}
