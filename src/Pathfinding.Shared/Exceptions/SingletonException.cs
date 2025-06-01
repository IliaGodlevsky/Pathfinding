namespace Pathfinding.Shared.Exceptions;

public class SingletonException : Exception
{
    private static string GetMessage(Type genericType)
    {
        return $"{genericType.Name} has neither private nor protected parametreless constructor";
    }

    internal SingletonException(Type type) : base(GetMessage(type))
    {

    }
}