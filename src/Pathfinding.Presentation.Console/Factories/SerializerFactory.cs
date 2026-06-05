using Autofac.Features.Metadata;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class SerializerFactory(Meta<Serializer>[] serializers) : ISerializerFactory
{
    private readonly Dictionary<SerializationFormat, Serializer> serializers = serializers.ToDictionary(
                x => (SerializationFormat)x.Metadata[MetadataKeys.ExportFormat],
                x => x.Value);

    public IReadOnlyList<SerializationFormat> AvailiableFormats { get; } =
        [..serializers
        .OrderBy(x => x.Metadata[MetadataKeys.Order])
        .Select(x => (SerializationFormat)x.Metadata[MetadataKeys.ExportFormat])];

    public Serializer Create(SerializationFormat format)
    {
        return serializers.GetValueOrDefault(format)
            ?? throw new KeyNotFoundException($"{format} was not found");
    }
}
