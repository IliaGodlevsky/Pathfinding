using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Factories;

internal interface ISerializerFactory
{
    IReadOnlyList<SerializationFormat> AvailiableFormats { get; }

    Serializer Create(SerializationFormat format);
}
