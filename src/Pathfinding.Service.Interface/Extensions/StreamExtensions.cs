using System.Runtime.Serialization;
using System.Text;

namespace Pathfinding.Service.Interface.Extensions;

public static class StreamExtensions
{
    public static async Task<T> ReadSerializableAsync<T>(this Stream stream, CancellationToken token = default)
        where T : IBinarySerializable, new()
    {
        T item = new();
        await item.DeserializeAsync(stream, token).ConfigureAwait(false);
        return item;
    }

    public static async Task<IReadOnlyCollection<T>> ReadSerializableArrayAsync<T>(this Stream stream, CancellationToken token = default)
        where T : IBinarySerializable, new()
    {
        int count = await stream.ReadInt32Async(token).ConfigureAwait(false);
        var list = new List<T>(count);
        while (count-- > 0)
        {
            var i = await stream.ReadSerializableAsync<T>(token).ConfigureAwait(false);
            list.Add(i);
        }
        return list.AsReadOnly();
    }

    public static async Task<double?> ReadNullableDoubleAsync(this Stream stream, CancellationToken token = default)
    {
        bool hasValue = await stream.ReadBoolAsync(token).ConfigureAwait(false);
        return hasValue ? await stream.ReadDoubleAsync(token).ConfigureAwait(false) : null;
    }

    public static async Task<int?> ReadNullableInt32Async(this Stream stream, CancellationToken token = default)
    {
        bool hasValue = await stream.ReadBoolAsync(token).ConfigureAwait(false);
        return hasValue ? await stream.ReadInt32Async(token).ConfigureAwait(false) : null;
    }

    public static async Task<string> ReadStringAsync(this Stream stream, CancellationToken token = default)
    {
        int length = await stream.ReadInt32Async(token).ConfigureAwait(false);
        if (length < 0)
        {
            throw new SerializationException($"Invalid string length: {length}");
        }

        if (length == 0)
        {
            return string.Empty;
        }

        var buffer = new byte[length];
        await stream.ReadExactlyAsync(buffer, token).ConfigureAwait(false);

        return Encoding.UTF8.GetString(buffer);
    }

    public static async Task<IReadOnlyList<int>> ReadArrayAsync(this Stream stream, CancellationToken token = default)
    {
        int count = await stream.ReadInt32Async(token).ConfigureAwait(false);
        var list = new List<int>(count);
        while (count-- > 0)
        {
            var i = await stream.ReadInt32Async(token).ConfigureAwait(false);
            list.Add(i);
        }
        return list.AsReadOnly();
    }

    public static async Task<int> ReadInt32Async(this Stream stream, CancellationToken token)
    {
        var buffer = new byte[4];
        await stream.ReadExactlyAsync(buffer, token).ConfigureAwait(false);
        return BitConverter.ToInt32(buffer, 0);
    }

    public static async Task<double> ReadDoubleAsync(this Stream stream, CancellationToken token)
    {
        var buffer = new byte[8];
        await stream.ReadExactlyAsync(buffer, token).ConfigureAwait(false);
        return BitConverter.ToDouble(buffer, 0);
    }

    public static async Task<bool> ReadBoolAsync(this Stream stream, CancellationToken token)
    {
        var buffer = new byte[1];
        await stream.ReadExactlyAsync(buffer, token).ConfigureAwait(false);
        return buffer[0] != 0;
    }

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