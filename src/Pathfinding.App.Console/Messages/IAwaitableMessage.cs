using System.Runtime.CompilerServices;

namespace Pathfinding.App.Console.Messages;

internal interface IAwaitableMessage
{
    TaskCompletionSource CreateCompletionSource();

    TaskAwaiter GetAwaiter();
}
