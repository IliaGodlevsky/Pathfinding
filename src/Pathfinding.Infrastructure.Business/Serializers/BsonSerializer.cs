using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Pathfinding.Infrastructure.Business.Serializers.Exceptions;
using Pathfinding.Service.Interface;

namespace Pathfinding.Infrastructure.Business.Serializers;

public sealed class BsonSerializer<T> : ISerializer<T>
{
    public async Task SerializeToAsync(T item, 
        Stream stream, CancellationToken token = default)
    {
        try
        {
            using var writer = new BsonDataWriter(stream);
            var serializer = new JsonSerializer();
            serializer.Serialize(writer, item);
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }

    public async Task<T> DeserializeFromAsync(Stream stream, 
        CancellationToken token = default)
    {
        try
        {
            using var reader = new BsonDataReader(stream);
            var serializer = new JsonSerializer();
            var obj = serializer.Deserialize<T>(reader);
            return await Task.FromResult(obj);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}