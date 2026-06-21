using Pathfinding.Serialization.Extensions;
using Pathfinding.Service.Interface;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Serialization.Models;

public record CoordinateSerializationModel : IBinarySerializable, IXmlSerializable
{
    public IReadOnlyCollection<int> Coordinate { get; set; }

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Coordinate = await stream.ReadArrayAsync(token).ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteInt32ArrayAsync(Coordinate, token).ConfigureAwait(false);
    }

    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        Coordinate = [.. reader.ReadElement<string>(nameof(Coordinate))
            .Split(',')
            .Select(int.Parse)];
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteElement(nameof(Coordinate), string.Join(",", Coordinate));
    }
}