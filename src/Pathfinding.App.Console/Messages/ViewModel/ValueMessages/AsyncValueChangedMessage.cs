using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages;

internal class AsyncValueChangedMessage<T, U>(T payload) 
    : ValueChangedMessage<T>(payload)
{
    private readonly TaskCompletionSource<U> source = new();

    public Task<U> Task => source.Task;

    public void SetCompleted(U result)
    {
        source.TrySetResult(result);
    }
}
