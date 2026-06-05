using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class SerializationFormatExtensions
{
    public static string ToExtensionRepresentation(this SerializationFormat exportFormat)
    {
        return exportFormat switch
        {
            SerializationFormat.Binary => ".dat",
            SerializationFormat.Json => ".json",
            SerializationFormat.Xml => ".xml",
            SerializationFormat.Csv => ".zip",
            SerializationFormat.MessagePack => ".msgpack",
            _ => string.Empty
        };
    }
}