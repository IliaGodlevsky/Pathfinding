using System.Reactive.Disposables;

namespace Pathfinding.App.Console.Models;

internal class StreamModel(Stream stream, 
    StreamFormat? format = null,
    params IDisposable[] disposables) : IDisposable, IAsyncDisposable
{
    public static readonly StreamModel Empty = new(Stream.Null);

    private readonly CompositeDisposable disposables = [.. disposables.Append(stream)];

    public Stream Stream { get; } = stream;

    public StreamFormat? Format { get; } = format;

    public bool IsEmpty => Stream == Stream.Null || !Format.HasValue;

    public void Dispose()
    {
        disposables.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var disposable in disposables)
        {
            if (disposable is IAsyncDisposable async)
            {
                await async.DisposeAsync();
            }
            else
            {
                disposable.Dispose();
            }
        }
        disposables.Dispose();
    }
}