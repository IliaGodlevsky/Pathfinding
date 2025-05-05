using Pathfinding.Domain.Core.Enums;
using Pathfinding.Service.Interface.Extensions;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Pathfinding.Service.Interface.Models.Serialization;

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

    public async Task DeserializeAsync(Stream stream, CancellationToken token = default)
    {
        Algorithm = (Algorithms)await stream.ReadInt32Async(token).ConfigureAwait(false);
        Heuristics = (Heuristics?)await stream.ReadNullableInt32Async(token).ConfigureAwait(false);
        Weight = await stream.ReadNullableDoubleAsync(token).ConfigureAwait(false);
        StepRule = (StepRules?)await stream.ReadNullableInt32Async(token).ConfigureAwait(false);
        ResultStatus = (RunStatuses)await stream.ReadInt32Async(token).ConfigureAwait(false);
        Elapsed = TimeSpan.FromMilliseconds(await stream.ReadDoubleAsync(token).ConfigureAwait(false));
        Steps = await stream.ReadInt32Async(token).ConfigureAwait(false);
        Cost = await stream.ReadDoubleAsync(token).ConfigureAwait(false);
        Visited = await stream.ReadInt32Async(token).ConfigureAwait(false);
    }

    public async Task SerializeAsync(Stream stream, CancellationToken token = default)
    {
        await stream.WriteInt32Async((int)Algorithm, token).ConfigureAwait(false);
        await stream.WriteNullableInt32Async((int?)Heuristics, token).ConfigureAwait(false);
        await stream.WriteNullableDoubleAsync(Weight, token).ConfigureAwait(false);
        await stream.WriteNullableInt32Async((int?)StepRule, token).ConfigureAwait(false);
        await stream.WriteInt32Async((int)ResultStatus, token).ConfigureAwait(false);
        await stream.WriteDoubleAsync(Elapsed.TotalMilliseconds, token).ConfigureAwait(false);
        await stream.WriteInt32Async(Steps, token).ConfigureAwait(false);
        await stream.WriteDoubleAsync(Cost, token).ConfigureAwait(false);
        await stream.WriteInt32Async(Visited, token).ConfigureAwait(false);
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