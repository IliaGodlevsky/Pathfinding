using System.Runtime.CompilerServices;

namespace Pathfinding.App.Console.Messages;

internal interface IAwaitableMessage
{
    TaskAwaiter GetAwaiter();

    void SetCompleted();
}