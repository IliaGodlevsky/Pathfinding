using Pathfinding.Serialization.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Serialization;

public sealed class BinarySerializer<T> : ISerializer<T>
    where T : IBinarySerializable, new()
{
    public async Task<T> DeserializeFromAsync(Stream stream,
        CancellationToken token = default)
    {
        try
        {
            var item = new T();
            await item.DeserializeAsync(stream, token).ConfigureAwait(false);
            return item;
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }

    public async Task SerializeToAsync(T item,
        Stream stream, CancellationToken token = default)
    {
        try
        {
            await item.SerializeAsync(stream, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
