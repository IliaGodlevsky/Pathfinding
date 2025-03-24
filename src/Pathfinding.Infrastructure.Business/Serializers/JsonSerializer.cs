using Newtonsoft.Json;
using Pathfinding.Infrastructure.Business.Serializers.Exceptions;
using Pathfinding.Service.Interface;
using System.Text;

namespace Pathfinding.Infrastructure.Business.Serializers;

public sealed class JsonSerializer<T> : ISerializer<T>
{
    public async Task<T> DeserializeFromAsync(Stream stream,
        CancellationToken token = default)
    {
        try
        {
            using var reader = new StreamReader(stream,
                Encoding.Default, false, 1024, leaveOpen: true);
            string deserialized = await reader.ReadToEndAsync(token)
                .ConfigureAwait(false);
            return await Task.Run(() => JsonConvert.DeserializeObject<T>(deserialized), token)
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
            using var writer = new StreamWriter(stream,
                Encoding.Default, 1024, leaveOpen: true);
            string serialized = await Task.Run(() => JsonConvert.SerializeObject(item), token)
                .ConfigureAwait(false);
            await writer.WriteAsync(serialized).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
