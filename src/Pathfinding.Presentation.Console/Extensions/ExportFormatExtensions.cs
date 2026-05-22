using Pathfinding.Presentation.Console.Models;

namespace Pathfinding.Presentation.Console.Extensions;

internal static class ExportFormatExtensions
{
    public static string ToExtensionRepresentation(this StreamFormat exportFormat)
    {
        return exportFormat switch
        {
            StreamFormat.Binary => ".dat",
            StreamFormat.Json => ".json",
            StreamFormat.Xml => ".xml",
            StreamFormat.Csv => ".zip",
            _ => string.Empty
        };
    }
}