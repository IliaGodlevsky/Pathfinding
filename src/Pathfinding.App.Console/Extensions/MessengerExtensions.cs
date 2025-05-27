using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages.ViewModel.ValueMessages;
using System.Runtime.CompilerServices;

// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    public static TaskAwaiter<U> GetAwaiter<T, U>(this AsyncValueChangedMessage<T, U> message)
    {
        return message.Task.GetAwaiter();
    }

    public static void RegisterAsyncHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, Func<object, TMessage, Task> handler)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, 
            async (r, msg) => await handler(r, msg).ConfigureAwait(false));
    }

    public static void RegisterAsyncHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<object, TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, 
            async (r, msg) => await handler(r, msg).ConfigureAwait(false));
    }
}
