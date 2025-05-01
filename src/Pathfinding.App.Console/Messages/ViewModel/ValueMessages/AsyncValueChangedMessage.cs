using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Pathfinding.App.Console.Messages.ViewModel.ValueMessages
{
    internal class AsyncValueChangedMessage<T>(T payload) 
        : ValueChangedMessage<T>(payload)
    {
        private readonly TaskCompletionSource<bool> source = new();

        public Task<bool> Task => source.Task;

        public void SetCompleted(bool result)
        {
            source.TrySetResult(result);
        }
    }
}
