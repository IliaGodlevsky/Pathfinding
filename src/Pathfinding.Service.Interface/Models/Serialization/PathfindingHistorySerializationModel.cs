using Pathfinding.Service.Interface.Extensions;
using Pathfinding.Service.Interface.Models.Undefined;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization
{
    public class PathfindingHistorySerializationModel : IBinarySerializable, IXmlSerializable
    {
        public GraphSerializationModel Graph { get; set; }

        public IReadOnlyCollection<VertexSerializationModel> Vertices { get; set; }

        public IReadOnlyCollection<RunStatisticsSerializationModel> Statistics { get; set; }

        public IReadOnlyCollection<CoordinateModel> Range { get; set; }

        public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
        {
            Graph = await stream.ReadSerializableAsync<GraphSerializationModel>(token).ConfigureAwait(false);
            Vertices = await stream.ReadSerializableArrayAsync<VertexSerializationModel>(token).ConfigureAwait(false);
            Statistics = await stream.ReadSerializableArrayAsync<RunStatisticsSerializationModel>(token).ConfigureAwait(false);
            Range = await stream.ReadSerializableArrayAsync<CoordinateModel>(token).ConfigureAwait(false);
        }

        public async Task SerializeAsync(Stream stream, CancellationToken token = default)
        {
            await stream.WriteAsync(Graph, token).ConfigureAwait(false);
            await stream.WriteAsync(Vertices, token).ConfigureAwait(false);
            await stream.WriteAsync(Statistics, token).ConfigureAwait(false);
            await stream.WriteAsync(Range, token).ConfigureAwait(false);
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            Graph = new GraphSerializationModel();
            Graph.ReadXml(reader);
            reader.Read();
            Vertices = reader.ReadCollection<VertexSerializationModel>(nameof(Vertices), "Vertex");
            Statistics = reader.ReadCollection<RunStatisticsSerializationModel>(nameof(Statistics), "Statistic");
            Range = reader.ReadCollection<CoordinateModel>(nameof(Range), "Coordinates");
        }

        public void WriteXml(XmlWriter writer)
        {
            Graph.WriteXml(writer);
            writer.WriteCollection(nameof(Vertices), "Vertex", Vertices);
            writer.WriteCollection(nameof(Statistics), "Statistic", Statistics);
            writer.WriteCollection(nameof(Range), "Coordinates", Range);
        }
    }
}
