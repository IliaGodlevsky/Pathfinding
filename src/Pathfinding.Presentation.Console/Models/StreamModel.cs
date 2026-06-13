namespace Pathfinding.Presentation.Console.Models;

internal sealed class StreamModel : IDisposable, IAsyncDisposable
{
    public static readonly StreamModel Empty = new();

    private readonly List<IDisposable> disposables;

    public Stream Stream { get; }

    public SerializationFormat? Format { get; }

    public bool NeedsCompress { get; }

    public bool IsEmpty { get; }

    public StreamModel(Stream stream = null,
        SerializationFormat? format = null,
        bool needsCompress = false,
        params IDisposable[] additionalDisposables)
    {
        Format = format;
        Stream = stream ?? Stream.Null;
        IsEmpty = Stream == Stream.Null || !Format.HasValue;
        disposables = [.. additionalDisposables, Stream];
        NeedsCompress = needsCompress;
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