using Pathfinding.Infrastructure.Business.Serializers.Exceptions;
using Pathfinding.Service.Interface;
using System.Text;
using System.Xml.Serialization;

namespace Pathfinding.Infrastructure.Business.Serializers;

public sealed class XmlSerializer<T> : ISerializer<T>
    where T : new()
{
    public async Task<T> DeserializeFromAsync(Stream stream, 
        CancellationToken token = default)
    {
        try
        {
            using var reader = new StreamReader(stream, 
                Encoding.UTF8, leaveOpen: true);
            var xml = await reader.ReadToEndAsync(token).ConfigureAwait(false);
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var stringReader = new StringReader(xml);
            return (T)xmlSerializer.Deserialize(stringReader);

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
            var xmlSerializer = new XmlSerializer(typeof(T));
            await using var stringWriter = new StringWriter();
            xmlSerializer.Serialize(stringWriter, item);
            var xml = stringWriter.ToString();
            await using var writer = new StreamWriter(stream, 
                Encoding.UTF8, leaveOpen: true);
            await writer.WriteAsync(xml.AsMemory(), token).ConfigureAwait(false);
            await writer.FlushAsync(token).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw new SerializationException(ex.Message, ex);
        }
    }
}
