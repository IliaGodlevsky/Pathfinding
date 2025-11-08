using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Exceptions;

[Category("Unit")]
public class PathfindingExceptionTests
{
    [Test]
    public void DefaultConstructor_SetsInnerExceptionToNull()
    {
        var exception = new PathfindingException();

        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void MessageConstructor_StoresMessage()
    {
        const string message = "custom";

        var exception = new PathfindingException(message);

        Assert.That(exception.Message, Is.EqualTo(message));
    }
}
