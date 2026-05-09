using Autofac.Features.Metadata;
using Pathfinding.App.Console.Injection;
using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Factories;

internal sealed class SerializerFactory(Meta<Serializer>[] serializers) : ISerializerFactory
{
    private readonly Dictionary<StreamFormat, Serializer> serializers = serializers.ToDictionary(
                x => (StreamFormat) x.Metadata[MetadataKeys.ExportFormat],
                x => x.Value);

    public IReadOnlyList<StreamFormat> AvailiableFormats { get; } = 
        [..serializers
        .OrderBy(x => x.Metadata[MetadataKeys.Order])
        .Select(x => (StreamFormat)x.Metadata[MetadataKeys.ExportFormat])];

    public Serializer CreateSerializer(StreamFormat format)
    {
        return serializers.GetValueOrDefault(format) 
            ?? throw new KeyNotFoundException($"{format} was not found");
    }
}
