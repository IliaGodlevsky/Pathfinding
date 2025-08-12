namespace Pathfinding.App.Console.Models;

internal sealed class StreamModel(Stream stream = null, StreamFormat? format = null,
    params IDisposable[] disposables) : IDisposable, IAsyncDisposable
{
    public static readonly StreamModel Empty = new();

    private readonly IDisposable[] disposables = disposables;

    public Stream Stream { get; } = stream ?? Stream.Null;

    public StreamFormat? Format { get; } = format;

    public bool IsEmpty => Stream == Stream.Null || !Format.HasValue;

    public void Dispose()
    {
        stream.Dispose();
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await stream.DisposeAsync().ConfigureAwait(false);
        foreach (var disposable in disposables)
        {
            if (disposable is IAsyncDisposable async)
            {
                await async.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                disposable.Dispose();
            }
        }
    }
}