using Pathfinding.Infrastructure.Business.Algorithms.Exceptions;

namespace Pathfinding.Infrastructure.Business.Tests.Algorithms.Exceptions;

[Category("Unit")]
public class DeadendVertexExceptionTests
{
    [Test]
    public void DefaultConstructor_UsesPredefinedMessage()
    {
        var exception = new DeadendVertexException();

        Assert.That(exception.Message, Is.EqualTo("Can't reach the destination"));
    }

    [Test]
    public void MessageConstructor_OverridesDefaultMessage()
    {
        const string message = "custom";

        var exception = new DeadendVertexException(message);

        Assert.That(exception.Message, Is.EqualTo(message));
    }
}
