using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Undefined;

public class VertexCostModel : IBinarySerializable, IXmlSerializable
{
    public int Cost { get; set; }

    public int UpperValueOfRange { get; set; }

    public int LowerValueOfRange { get; set; }

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Cost = await stream.ReadInt32Async(token).ConfigureAwait(false);
        UpperValueOfRange = await stream.ReadInt32Async(token).ConfigureAwait(false);
        LowerValueOfRange = await stream.ReadInt32Async(token).ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteInt32Async(Cost, token).ConfigureAwait(false);
        await stream.WriteInt32Async(UpperValueOfRange, token).ConfigureAwait(false);
        await stream.WriteInt32Async(LowerValueOfRange, token).ConfigureAwait(false);
    }

    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        Cost = reader.ReadElement<int>(nameof(Cost));
        UpperValueOfRange = reader.ReadElement<int>(nameof(UpperValueOfRange));
        LowerValueOfRange = reader.ReadElement<int>(nameof(LowerValueOfRange));
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteElement(nameof(Cost), Cost);
        writer.WriteElement(nameof(UpperValueOfRange), UpperValueOfRange);
        writer.WriteElement(nameof(LowerValueOfRange), LowerValueOfRange);
    }
}