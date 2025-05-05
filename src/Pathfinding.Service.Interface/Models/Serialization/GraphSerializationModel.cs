using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization;

public record GraphSerializationModel : IBinarySerializable, IXmlSerializable
{
    public string Name { get; set; }

    public SmoothLevels SmoothLevel { get; set; }

    public Neighborhoods Neighborhood { get; set; }

    public GraphStatuses Status { get; set; }

    public IReadOnlyList<int> DimensionSizes { get; set; }

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Name = await stream.ReadStringAsync(token).ConfigureAwait(false);
        SmoothLevel = (SmoothLevels)await stream.ReadInt32Async(token).ConfigureAwait(false);
        Neighborhood = (Neighborhoods)await stream.ReadInt32Async(token).ConfigureAwait(false);
        Status = (GraphStatuses)await stream.ReadInt32Async(token).ConfigureAwait(false);
        DimensionSizes = await stream.ReadArrayAsync(token).ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteStringAsync(Name, token).ConfigureAwait(false);
        await stream.WriteInt32Async((int)SmoothLevel, token).ConfigureAwait(false);
        await stream.WriteInt32Async((int)Neighborhood, token).ConfigureAwait(false);
        await stream.WriteInt32Async((int)Status, token).ConfigureAwait(false);
        await stream.WriteInt32ArrayAsync(DimensionSizes, token).ConfigureAwait(false);
    }

    public XmlSchema GetSchema() => null;

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteAttribute(nameof(Name), Name);
        writer.WriteAttribute(nameof(SmoothLevel), SmoothLevel);
        writer.WriteAttribute(nameof(Neighborhood), Neighborhood);
        writer.WriteAttribute(nameof(Status), Status);
        writer.WriteAttribute(nameof(DimensionSizes), string.Join(",", DimensionSizes));
    }

    public void ReadXml(XmlReader reader)
    {
        Name = reader.ReadAttribute<string>(nameof(Name));
        SmoothLevel = reader.ReadEnumAttribute<SmoothLevels>(nameof(SmoothLevel));
        Neighborhood = reader.ReadEnumAttribute<Neighborhoods>(nameof(Neighborhood));
        Status = reader.ReadEnumAttribute<GraphStatuses>(nameof(Status));
        DimensionSizes = Array.ConvertAll(reader.ReadAttribute<string>(nameof(DimensionSizes)).Split(','), int.Parse);
    }
}