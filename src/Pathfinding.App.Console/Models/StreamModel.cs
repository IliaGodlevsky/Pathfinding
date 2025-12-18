namespace Pathfinding.App.Console.Models;

internal sealed class StreamModel : IDisposable, IAsyncDisposable
{
    public static readonly StreamModel Empty = new();

    private readonly List<IDisposable> disposables;

    public Stream Stream { get; }

    public StreamFormat? Format { get; }

    public bool IsEmpty { get; }

    public StreamModel(Stream stream = null,
        StreamFormat? format = null,
        params IDisposable[] additionalDisposables)
    {
        Stream = stream ?? Stream.Null;
        Format = format;
        IsEmpty = Stream == Stream.Null || !Format.HasValue;
        disposables = [.. additionalDisposables, Stream];
    }

    public void Dispose()
    {
        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
        disposables.Clear();
    }

    public async ValueTask DisposeAsync()
    {
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
        disposables.Clear();
    }
}