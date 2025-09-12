using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    private sealed class UnsubcribeHandler(Action handler) : IDisposable
    {
        public void Dispose() => handler();
    }

    public static IDisposable RegisterAwaitHandler<TMessage, TToken>(
        this IMessenger messenger,
        object recipient, 
        TToken token, 
        Func<TMessage, Task> handler)
        where TMessage : class, IAwaitableMessage
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, 
            async (_, msg) =>
            {
                await handler(msg).ConfigureAwait(false);
                msg.SetCompleted();
            });

        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage, TToken>(recipient, token));
    }

    public static IDisposable RegisterHandler<TMessage>(
        this IMessenger messenger, 
        object recipient, 
        Action<TMessage> action)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, (_, msg) => action(msg));
        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage>(recipient));
    }

    public static IDisposable RegisterHandler<TMessage, TToken>(
        this IMessenger messenger,
        object recipient, 
        TToken token, 
        Action<TMessage> action)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, (_, msg) => action(msg));
        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage, TToken>(recipient, token));
    }

    public static IDisposable RegisterAsyncHandler<TMessage>(
        this IMessenger messenger,
        object recipient, 
        Func<TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, async (_, msg) 
            => await handler(msg).ConfigureAwait(false));
        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage>(recipient));
    }
}
