using CommunityToolkit.Mvvm.Messaging;
using Pathfinding.App.Console.Messages;
using System.Reactive;
// ReSharper disable AsyncVoidLambda

namespace Pathfinding.App.Console.Extensions;

internal static class MessengerExtensions
{
    public static async Task SendAsync<TMessage, TToken>(this IMessenger messenger,
        TMessage message, TToken token)
        where TMessage : class, IAsyncMessage<Unit>
        where TToken : IEquatable<TToken>
    {
        var tcs = new TaskCompletionSource<Unit>();
        void Signal(Unit unit) => tcs.TrySetResult(unit);
        message.Signal = Signal;
        var timeout = Task.Delay(TimeSpan.FromSeconds(100));
        messenger.Send(message, token);
        await Task.WhenAny(timeout, tcs.Task).ConfigureAwait(false);
    }

    public static void RegisterAsyncHandler<TMessage, TToken>(this IMessenger messenger,
        object recipient, TToken token, Func<object, TMessage, Task> handler)
        where TMessage : class
        where TToken : IEquatable<TToken>
    {
        messenger.Register<TMessage, TToken>(recipient, token, async (r, msg) => await handler(r, msg).ConfigureAwait(false));
    }

    public static void RegisterAsyncHandler<TMessage>(this IMessenger messenger,
        object recipient, Func<object, TMessage, Task> handler)
        where TMessage : class
    {
        messenger.Register<TMessage>(recipient, async (r, msg) => await handler(r, msg).ConfigureAwait(false));
    }
}
