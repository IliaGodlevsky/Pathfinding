using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization
{
    public class PathfindingHisotiriesSerializationModel : IBinarySerializable, IXmlSerializable
    {
        public IReadOnlyCollection<PathfindingHistorySerializationModel> Histories { get; set; } = [];

        public void Deserialize(BinaryReader reader)
        {
            Histories = reader.ReadSerializableArray<PathfindingHistorySerializationModel>();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Histories);
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
    }
}
