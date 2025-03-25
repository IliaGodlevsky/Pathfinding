using Pathfinding.Infrastructure.Business.Serializers.Exceptions;
using Pathfinding.Service.Interface;
using System.Text;
using System.Xml.Serialization;

namespace Pathfinding.Infrastructure.Business.Serializers;

public sealed class XmlSerializer<T> : ISerializer<T>
    where T : IXmlSerializable, new()
{
    public async Task<T> DeserializeFromAsync(Stream stream,
        CancellationToken token = default)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new StreamReader(stream,
                Encoding.Default, false, 1024, leaveOpen: true);
            var result = await Task.Run(() => serializer.Deserialize(reader), token)
                .ConfigureAwait(false);
            return (T)result;
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
            var serializer = new XmlSerializer(typeof(T));
            using var writer = new StreamWriter(stream,
                Encoding.Default, 1024, leaveOpen: true);
            await Task.Run(() => serializer.Serialize(writer, item), token)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
