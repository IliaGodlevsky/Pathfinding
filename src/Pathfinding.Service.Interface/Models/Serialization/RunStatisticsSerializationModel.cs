using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization
{
    public class RunStatisticsSerializationModel : IBinarySerializable, IXmlSerializable
    {
        public Algorithms Algorithm { get; set; }

        public Heuristics? Heuristics { get; set; } = null;

        public double? Weight { get; set; } = null;

        public StepRules? StepRule { get; set; } = null;

        public RunStatuses ResultStatus { get; set; }

        public TimeSpan Elapsed { get; set; }

        public int Steps { get; set; }

        public double Cost { get; set; }

        public int Visited { get; set; }

        public void Deserialize(BinaryReader reader)
        {
            Algorithm = (Algorithms)reader.ReadInt32();
            Heuristics = (Heuristics?)reader.ReadNullableInt32();
            Weight = reader.ReadNullableDouble();
            StepRule = (StepRules?)reader.ReadNullableInt32();
            ResultStatus = (RunStatuses)reader.ReadInt32();
            Elapsed = TimeSpan.FromMilliseconds(reader.ReadDouble());
            Steps = reader.ReadInt32();
            Cost = reader.ReadDouble();
            Visited = reader.ReadInt32();
        }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((int)Algorithm);
            writer.WriteNullableInt32((int?)Heuristics);
            writer.Write(Weight);
            writer.WriteNullableInt32((int?)StepRule);
            writer.Write((int)ResultStatus);
            writer.Write(Elapsed.TotalMilliseconds);
            writer.Write(Steps);
            writer.Write(Cost);
            writer.Write(Visited);
        }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            Algorithm = reader.ReadEnumElement<Algorithms>(nameof(Algorithm));
            Heuristics = reader.ReadNullableEnum<Heuristics>(nameof(Heuristics));
            Weight = reader.ReadNullableElement<double>(nameof(Weight));
            StepRule = reader.ReadNullableEnum<StepRules>(nameof(StepRule));
            ResultStatus = reader.ReadEnumElement<RunStatuses>(nameof(ResultStatus));
            Elapsed = TimeSpan.FromMilliseconds(reader.ReadElement<double>(nameof(Elapsed)));
            Steps = reader.ReadElement<int>(nameof(Steps));
            Cost = reader.ReadElement<double>(nameof(Cost));
            Visited = reader.ReadElement<int>(nameof(Visited));
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElement(nameof(Algorithm), Algorithm.ToString());
            writer.WriteNullableElement(nameof(Heuristics), Heuristics);
            writer.WriteNullableElement(nameof(Weight), Weight);
            writer.WriteNullableElement(nameof(StepRule), StepRule);
            writer.WriteElement(nameof(ResultStatus), ResultStatus.ToString());
            writer.WriteElement(nameof(Elapsed), Elapsed.TotalMilliseconds);
            writer.WriteElement(nameof(Steps), Steps);
            writer.WriteElement(nameof(Cost), Cost);
            writer.WriteElement(nameof(Visited), Visited);
        }
    }
}
