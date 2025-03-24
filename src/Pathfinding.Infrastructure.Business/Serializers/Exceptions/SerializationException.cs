namespace Pathfinding.Infrastructure.Business.Serializers.Exceptions;

public class SerializationException : SystemException
{
    public SerializationException(string message)
        : base(message)
    {

    }

    public SerializationException(string message, Exception innerException)
        : base(message, innerException)
    {

    }
}