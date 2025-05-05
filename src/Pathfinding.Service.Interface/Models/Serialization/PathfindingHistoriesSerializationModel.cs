using Pathfinding.Service.Interface.Extensions;
using System.IO.Compression;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization;

public class PathfindingHistoriesSerializationModel
    : IBinarySerializable, IXmlSerializable, IBundleSerializable
{
    public IReadOnlyCollection<PathfindingHistorySerializationModel> Histories { get; set; } = [];

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Histories = await stream.ReadSerializableArrayAsync<PathfindingHistorySerializationModel>(token)
            .ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteAsync(Histories, token).ConfigureAwait(false);
    }

    public XmlSchema GetSchema() => null;

    public void ReadXml(XmlReader reader)
    {
        reader.Read();
        Histories = reader.ReadCollection<PathfindingHistorySerializationModel>(nameof(Histories), "Graph");
    }

    public void WriteXml(XmlWriter writer)
    {
        writer.WriteCollection(nameof(Histories), "Graph", Histories);
    }

    public async Task SerializeAsync(ZipArchive archive, CancellationToken token = default)
    {
        await archive.WriteHistoryAsync(Histories, token).ConfigureAwait(false);
    }

    public async Task DeserializeAsync(ZipArchive archive, CancellationToken token = default)
    {
        Histories = await archive.ReadHistoryAsync(token).ConfigureAwait(false);
    }
}