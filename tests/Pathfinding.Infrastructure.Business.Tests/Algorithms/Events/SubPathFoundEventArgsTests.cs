using Pathfinding.Infrastructure.Business.Algorithms.Events;
using Pathfinding.Infrastructure.Business.Algorithms.GraphPaths;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Events;

[Category("Unit")]
public class SubPathFoundEventArgsTests
{
    [Test]
    public void Constructor_ExposesSubPath()
    {
        var subPath = NullGraphPath.Instance;

        var args = new SubPathFoundEventArgs(subPath);

        Assert.That(args.SubPath, Is.SameAs(subPath));
    }
}
