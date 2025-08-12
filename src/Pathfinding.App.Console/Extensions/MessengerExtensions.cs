using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    private sealed class UnregisterTokenizedHandler<TMessage, TToken>(
        IMessenger messenger, object recipient, TToken token) : IDisposable
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        public void Dispose()
        {
            messenger.Unregister<TMessage, TToken>(recipient, token);
        }
    }

    private sealed class UnregisterHandler<TMessage>(
        IMessenger messenger, object recipient) : IDisposable
        where TMessage : class
    {
        public void Dispose()
        {
            messenger.Unregister<TMessage>(recipient);
        }
    }

    public static IDisposable RegisterAwaitHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, Func<object, TMessage, Task> handler)
        where TMessage : class, IAwaitMessage
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, 
            async (r, msg) =>
            {
                await handler(r, msg).ConfigureAwait(false);
                msg.SetCompleted();
            });

        return new UnregisterTokenizedHandler<TMessage, TToken>(messenger, recipient, token);
    }

    public static IDisposable RegisterHandler<TMessage>(this IMessenger messenger, 
        object recipient, MessageHandler<object, TMessage> action)
        where TMessage : class
    {
        messenger.Register(recipient, action);
        return new UnregisterHandler<TMessage>(messenger, recipient);
    }

    public static IDisposable RegisterHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, MessageHandler<object, TMessage> action)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, action);
        return new UnregisterTokenizedHandler<TMessage, TToken>(messenger, recipient, token);
    }

    public static IDisposable RegisterAsyncHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<object, TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, async (r, msg) 
            => await handler(r, msg).ConfigureAwait(false));
        return new UnregisterHandler<TMessage>(messenger, recipient);
    }
}
