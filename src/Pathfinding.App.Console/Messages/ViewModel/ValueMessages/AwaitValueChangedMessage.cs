using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Runtime.CompilerServices;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal abstract class AwaitValueChangedMessage<T>(T payload) 
    : ValueChangedMessage<T>(payload), IAwaitMessage
{
    private readonly TaskCompletionSource source = new();

    public TaskAwaiter GetAwaiter()
    {
        return source.Task.GetAwaiter();
    }

    public void SetCompleted()
    {
        source.TrySetResult();
    }
}
