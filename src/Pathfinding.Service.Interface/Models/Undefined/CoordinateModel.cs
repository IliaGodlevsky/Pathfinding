using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Undefined
{
    public record CoordinateModel : IBinarySerializable, IXmlSerializable
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
            var coordinates = reader.ReadElement<string>(nameof(Coordinate))
                                    .Split(',')
                                    .Select(int.Parse)
                                    .ToList();
            Coordinate = coordinates;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement(nameof(Coordinate), string.Join(",", Coordinate));
        }
    }
}
