using System.Reactive.Disposables;

namespace Pathfinding.Presentation.Console.Models;

internal sealed class StreamModel : IDisposable, IAsyncDisposable
{
    public static readonly StreamModel Empty = new();

    private readonly CompositeDisposable disposables;

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
        disposables.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        disposables.Dispose();
        return ValueTask.CompletedTask;
    }
}