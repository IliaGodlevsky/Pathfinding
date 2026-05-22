using System.Runtime.CompilerServices;

namespace Pathfinding.Presentation.Console.Messages;

internal interface IAwaitableMessage
{
    TaskCompletionSource CreateCompletionSource();

    TaskAwaiter GetAwaiter();
}
