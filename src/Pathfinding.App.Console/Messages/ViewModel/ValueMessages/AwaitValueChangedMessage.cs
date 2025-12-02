using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Runtime.CompilerServices;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal abstract class AwaitValueChangedMessage<T>(T payload)
    : ValueChangedMessage<T>(payload), IAwaitableMessage
{
    private readonly List<Task> tasks = [];

    public virtual TaskCompletionSource CreateCompletionSource()
    {
        var tcs = new TaskCompletionSource();
        tasks.Add(tcs.Task);
        return tcs;
    }

    public virtual TaskAwaiter GetAwaiter()
    {
        return Task.WhenAll(tasks).GetAwaiter();
    }
}
