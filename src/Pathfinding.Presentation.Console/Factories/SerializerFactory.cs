using Autofac.Features.Metadata;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;
using System.Collections.Frozen;

using CompressSerializer = Pathfinding.Serialization.Decorators.CompressSerializer<Pathfinding.Serialization.Models.PathfindingHistoriesSerializationModel>;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class SerializerFactory(Meta<Serializer>[] serializers) : ISerializerFactory
{
    private readonly IReadOnlyDictionary<SerializationFormat, Serializer> serializers 
        = serializers.ToFrozenDictionary(
            x => (SerializationFormat)x.Metadata[MetadataKeys.ExportFormat],
            x => x.Value);

    public IReadOnlyList<SerializationFormat> AvailiableFormats { get; } =
        [..serializers
            .OrderBy(x => x.Metadata[MetadataKeys.Order])
            .Select(x => (SerializationFormat)x.Metadata[MetadataKeys.ExportFormat])];

    public Serializer Create(StreamModel streamModel)
    {
        var format = streamModel.Format!.Value;
        var serializer = serializers.GetValueOrDefault(format)
            ?? throw new KeyNotFoundException($"{format} was not found");
        return streamModel.NeedsCompress 
            ? new CompressSerializer(serializer)
            : serializer;
    }
}
