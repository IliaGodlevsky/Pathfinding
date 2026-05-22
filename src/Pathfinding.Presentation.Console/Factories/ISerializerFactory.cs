using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Factories;

internal interface ISerializerFactory
{
    IReadOnlyList<StreamFormat> AvailiableFormats { get; }

    Serializer Create(StreamFormat format);
}
