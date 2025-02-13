using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization
{
    public record GraphSerializationModel : IBinarySerializable, IXmlSerializable
    {
        public string Name { get; set; }

        public SmoothLevels SmoothLevel { get; set; }

        public Neighborhoods Neighborhood { get; set; }

        public GraphStatuses Status { get; set; }

        public IReadOnlyList<int> DimensionSizes { get; set; }

        public IReadOnlyCollection<VertexSerializationModel> Vertices { get; set; }

        public void Deserialize(BinaryReader reader)
        {
            Name = reader.ReadString();
            SmoothLevel = (SmoothLevels)reader.ReadInt32();
            Neighborhood = (Neighborhoods)reader.ReadInt32();
            Status = (GraphStatuses)reader.ReadInt32();
            DimensionSizes = reader.ReadArray();
            Vertices = reader.ReadSerializableArray<VertexSerializationModel>();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Name);
            writer.Write((int)SmoothLevel);
            writer.Write((int)Neighborhood);
            writer.Write((int)Status);
            writer.Write(DimensionSizes);
            writer.Write(Vertices);
        }

        public XmlSchema GetSchema() => null;

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttribute(nameof(Name), Name);
            writer.WriteAttribute(nameof(SmoothLevel), SmoothLevel);
            writer.WriteAttribute(nameof(Neighborhood), Neighborhood);
            writer.WriteAttribute(nameof(Status), Status);
            writer.WriteAttribute(nameof(DimensionSizes), string.Join(",", DimensionSizes));
            writer.WriteCollection(nameof(Vertices), "Vertex", Vertices);
        }

        public void ReadXml(XmlReader reader)
        {
            Name = reader.ReadAttribute<string>(nameof(Name));
            SmoothLevel = reader.ReadEnumAttribute<SmoothLevels>(nameof(SmoothLevel));
            Neighborhood = reader.ReadEnumAttribute<Neighborhoods>(nameof(Neighborhood));
            Status = reader.ReadEnumAttribute<GraphStatuses>(nameof(Status));
            DimensionSizes = Array.ConvertAll(reader.ReadAttribute<string>(nameof(DimensionSizes)).Split(','), int.Parse);
            reader.Read();
            Vertices = reader.ReadCollection<VertexSerializationModel>(nameof(Vertices), "Vertex");
        }
    }
}
