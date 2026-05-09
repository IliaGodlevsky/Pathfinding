using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Factories;

internal interface ISerializerFactory
{
    IReadOnlyList<StreamFormat> AvailiableFormats { get; }

    Serializer CreateSerializer(StreamFormat format);
}
