using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    public static void RegisterAwaitHandler<TMessage, TToken>(this IMessenger messenger,
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
    }

    public static void RegisterAsyncHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<object, TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, async (r, msg) 
            => await handler(r, msg).ConfigureAwait(false));
    }
}
