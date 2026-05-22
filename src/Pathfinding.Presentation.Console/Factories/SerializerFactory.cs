using Autofac.Features.Metadata;
using Pathfinding.Presentation.Console.Injection;
using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Factories;

internal sealed class SerializerFactory(Meta<Serializer>[] serializers) : ISerializerFactory
{
    private readonly Dictionary<StreamFormat, Serializer> serializers = serializers.ToDictionary(
                x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat],
                x => x.Value);

    public IReadOnlyList<StreamFormat> AvailiableFormats { get; } =
        [..serializers
        .OrderBy(x => x.Metadata[MetadataKeys.Order])
        .Select(x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat])];

    public Serializer Create(StreamFormat format)
    {
        return serializers.GetValueOrDefault(format)
            ?? throw new KeyNotFoundException($"{format} was not found");
    }
}
