namespace Pathfinding.Service.Algorithms.Exceptions;

public class PathfindingException : Exception
{
    public PathfindingException()
    {

    }

    public PathfindingException(string message) : base(message)
    {

    }

    public PathfindingException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}
