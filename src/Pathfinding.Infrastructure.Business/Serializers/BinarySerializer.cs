using Pathfinding.Infrastructure.Business.Serializers.Exceptions;
using Pathfinding.Service.Interface;
using Pathfinding.Service.Interface.Extensions;
using System.Text;

namespace Pathfinding.Infrastructure.Business.Serializers;

public sealed class BinarySerializer<T> : ISerializer<T>
    where T : IBinarySerializable, new()
{
    public async Task<T> DeserializeFromAsync(Stream stream,
        CancellationToken token = default)
    {
        try
        {
            using var reader = new BinaryReader(stream,
                Encoding.Default, leaveOpen: true);
            return await Task.Run(reader.ReadSerializable<T>, token)
                .ConfigureAwait(false);
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
            using var writer = new BinaryWriter(stream,
                Encoding.Default, leaveOpen: true);
            await Task.Run(() => writer.Write(item), token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
