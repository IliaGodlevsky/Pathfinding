using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    private sealed class UnsubcribeHandler(Action handler) : IDisposable
    {
        public void Dispose() => handler?.Invoke();
    }

    public static IDisposable RegisterAwaitHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, Func<object, TMessage, Task> handler)
        where TMessage : class, IAwaitableMessage
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, 
            async (r, msg) =>
            {
                await handler(r, msg).ConfigureAwait(false);
                msg.SetCompleted();
            });

        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage, TToken>(recipient, token));
    }

    public static IDisposable RegisterHandler<TMessage>(this IMessenger messenger, 
        object recipient, MessageHandler<object, TMessage> action)
        where TMessage : class
    {
        messenger.Register(recipient, action);
        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage>(recipient));
    }

    public static IDisposable RegisterHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, MessageHandler<object, TMessage> action)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, action);
        return new UnsubcribeHandler(()
            => messenger.Unregister<TMessage, TToken>(recipient, token));
    }

    public static IDisposable RegisterAsyncHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<object, TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, async (r, msg) 
            => await handler(r, msg).ConfigureAwait(false));
        return new UnsubcribeHandler(() 
            => messenger.Unregister<TMessage>(recipient));
    }
}
