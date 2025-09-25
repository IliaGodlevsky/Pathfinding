using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    private sealed class UnregisterAdapter(Action handler) : IDisposable
    {
        public static UnregisterAdapter New<TMessage, TToken>(IMessenger messenger,
            object recipient, TToken token)
            where TMessage : class
            where TToken : IEquatable<TToken>
        {
            return new(() => messenger.Unregister<TMessage, TToken>(recipient, token));
        }

        public static UnregisterAdapter New<TMessage>(IMessenger messenger, object recipient)
            where TMessage : class
        {
            return new(() => messenger.Unregister<TMessage>(recipient));
        }

        public void Dispose() => handler();
    }

    public static IDisposable RegisterAwaitHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, Func<TMessage, Task> handler)
        where TMessage : class, IAwaitableMessage
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, 
            async (_, msg) =>
            {
                await handler(msg).ConfigureAwait(false);
                msg.SetCompleted();
            });

        return UnregisterAdapter.New<TMessage, TToken>(messenger, recipient, token);
    }

    public static IDisposable RegisterHandler<TMessage>(this IMessenger messenger, 
        object recipient, Action<TMessage> action)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, (_, msg) => action(msg));
        return UnregisterAdapter.New<TMessage>(messenger, recipient);
    }

    public static IDisposable RegisterHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, Action<TMessage> action)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, (_, msg) => action(msg));
        return UnregisterAdapter.New<TMessage, TToken>(messenger, recipient, token);
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
