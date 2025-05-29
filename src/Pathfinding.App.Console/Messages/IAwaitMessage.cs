using System.Runtime.CompilerServices;

namespace Pathfinding.App.Console.Messages;

internal interface IAwaitMessage
{
    TaskAwaiter GetAwaiter();

    void SetCompleted();
}