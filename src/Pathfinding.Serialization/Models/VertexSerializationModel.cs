using Pathfinding.Serialization.Extensions;
using Pathfinding.Service.Interface;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Serialization.Models;

public class VertexSerializationModel : IBinarySerializable, IXmlSerializable
{
    public CoordinateSerializationModel Position { get; set; }

    public VertexCostSerializationModel Cost { get; set; }

    public bool IsObstacle { get; set; }

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Position = await stream.ReadSerializableAsync<CoordinateSerializationModel>(token).ConfigureAwait(false);
        Cost = await stream.ReadSerializableAsync<VertexCostSerializationModel>(token).ConfigureAwait(false);
        IsObstacle = await stream.ReadBoolAsync(token).ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteAsync(Position, token).ConfigureAwait(false);
        await stream.WriteAsync(Cost, token).ConfigureAwait(false);
        await stream.WriteBoolAsync(IsObstacle, token).ConfigureAwait(false);
    }

    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        Position = new CoordinateSerializationModel();
        Position.ReadXml(reader);
        Cost = new VertexCostSerializationModel();
        Cost.ReadXml(reader);
        IsObstacle = reader.ReadElement<bool>(nameof(IsObstacle));
    }

    public void WriteXml(XmlWriter writer)
    {
        Position.WriteXml(writer);
        Cost.WriteXml(writer);
        writer.WriteElement(nameof(IsObstacle), IsObstacle);
    }
}