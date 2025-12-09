using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    private sealed class UnregisterAdapter(Action handler) : IDisposable
    {
        public static IDisposable New<TMessage>(IMessenger messenger, object recipient)
            where TMessage : class
        {
            return new UnregisterAdapter(()
                => messenger.Unregister<TMessage>(recipient));
        }

        public void Dispose() => handler();
    }

    public static IDisposable RegisterAwaitHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<TMessage, Task> handler)
        where TMessage : class, IAwaitableMessage
    {
        messenger.Register<TMessage>(recipient,
            async (_, msg) =>
            {
                var tcs = msg.CreateCompletionSource();
                await handler(msg).ConfigureAwait(false);
                tcs.TrySetResult();
            });

        return UnregisterAdapter.New<TMessage>(messenger, recipient);
    }

    public static IDisposable RegisterHandler<TMessage>(this IMessenger messenger,
        object recipient, Action<TMessage> action)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, (_, msg) => action(msg));
        return UnregisterAdapter.New<TMessage>(messenger, recipient);
    }

    public static IDisposable RegisterAsyncHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, async (_, msg)
            => await handler(msg).ConfigureAwait(false));
        return UnregisterAdapter.New<TMessage>(messenger, recipient);
    }
}
