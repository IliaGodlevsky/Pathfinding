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
            var json = await reader.ReadToEndAsync(token).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<T>(json);
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
            await using var writer = new StreamWriter(stream,
                Encoding.Default, 1024, leaveOpen: true);
            var json = JsonConvert.SerializeObject(item);
            await writer.WriteAsync(json).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
