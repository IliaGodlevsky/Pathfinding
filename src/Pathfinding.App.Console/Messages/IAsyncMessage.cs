namespace Pathfinding.App.Console.Messages;

internal interface IAsyncMessage<T>
{
    Action<T> Signal { get; set; }
}
