namespace Pathfinding.Infrastructure.Business.Algorithms.Exceptions;

public class DeadendVertexException : PathfindingException
{
    private const string Msg = "Can't reach the destination";

    public DeadendVertexException() : base(Msg)
    {

    }

    public DeadendVertexException(string message) : base(message)
    {

    }

    public DeadendVertexException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
