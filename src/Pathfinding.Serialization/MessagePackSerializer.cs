using Pathfinding.Serialization.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Serialization;

public sealed class MessagePackSerializer<T> : ISerializer<T>
    where T : IMessagePackSerializable, new()
{
    public async Task<T> DeserializeFromAsync(Stream stream, 
        CancellationToken token = default)
    {
        try
        {
            var item = new T();
            await item.DeserializePackAsync(stream, token).ConfigureAwait(false);
            return item;
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }

    public async Task SerializeToAsync(T item, Stream stream,
        CancellationToken token = default)
    {
        try
        {
            await item.SerializePackAsync(stream, token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
