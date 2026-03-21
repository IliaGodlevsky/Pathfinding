using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Undefined;

public class VertexCostModel : IBinarySerializable, IXmlSerializable
{
    public int Cost { get; set; }

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Cost = await stream.ReadInt32Async(token).ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteInt32Async(Cost, token).ConfigureAwait(false);
    }

    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        Cost = reader.ReadElement<int>(nameof(Cost));
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteElement(nameof(Cost), Cost);
    }
}