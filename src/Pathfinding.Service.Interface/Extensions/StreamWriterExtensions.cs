using System.Text;

namespace Pathfinding.Service.Interface.Extensions;

public static class StreamWriterExtensions
{
    public static async Task WriteAsync(this Stream stream, 
        IReadOnlyCollection<IBinarySerializable> collection, 
        CancellationToken token = default)
    {
        await stream
            .WriteInt32Async(collection.Count, token)
            .ConfigureAwait(false);
        foreach (var serializable in collection)
        {
            await serializable
                .SerializeAsync(stream, token)
                .ConfigureAwait(false);
        }
    }

    public static async Task WriteAsync(this Stream stream, 
        IBinarySerializable serializable,
        CancellationToken token = default)
    {
        await serializable
            .SerializeAsync(stream, token)
            .ConfigureAwait(false);
    }

    public static async Task WriteNullableInt32Async(this Stream stream, 
        int? value, 
        CancellationToken token = default)
    {
        await stream
            .WriteBoolAsync(value.HasValue, token)
            .ConfigureAwait(false);
        if (value.HasValue)
        {
            await stream
                .WriteInt32Async(value.Value, token)
                .ConfigureAwait(false);
        }
    }

    public static async Task WriteNullableDoubleAsync(this Stream stream, 
        double? value, 
        CancellationToken token = default)
    {
        await stream
            .WriteBoolAsync(value.HasValue, token)
            .ConfigureAwait(false);
        if (value.HasValue)
        {
            await stream
                .WriteDoubleAsync(value.Value, token)
                .ConfigureAwait(false);
        }
    }

    public static async Task WriteInt32ArrayAsync(this Stream stream, 
        IReadOnlyCollection<int> array,
        CancellationToken token = default)
    {
        await stream
            .WriteInt32Async(array.Count, token)
            .ConfigureAwait(false);
        foreach (var item in array)
        {
            await stream
                .WriteInt32Async(item, token)
                .ConfigureAwait(false);
        }
    }

    public static async Task WriteInt32Async(this Stream stream, int value,
        CancellationToken token)
    {
        var buffer = BitConverter.GetBytes(value);
        await stream.WriteAsync(buffer, token).ConfigureAwait(false);
    }

    public static async Task WriteDoubleAsync(this Stream stream, double value,
        CancellationToken token)
    {
        var buffer = BitConverter.GetBytes(value);
        await stream.WriteAsync(buffer, token).ConfigureAwait(false);
    }

    public static async Task WriteStringAsync(this Stream stream, string value, 
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(value, nameof(value));

        var buffer = Encoding.UTF8.GetBytes(value);
        await stream.WriteInt32Async(buffer.Length, token).ConfigureAwait(false);
        await stream.WriteAsync(buffer, token).ConfigureAwait(false);
    }

    public static async Task WriteBoolAsync(this Stream stream, bool value, 
        CancellationToken token)
    {
        var buffer = new[] { (byte)(value ? 1 : 0) };
        await stream.WriteAsync(buffer, token).ConfigureAwait(false);
    }
}