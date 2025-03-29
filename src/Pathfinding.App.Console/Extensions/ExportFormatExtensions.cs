using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Extensions;

internal static class ExportFormatExtensions
{
    public static string ToExtensionRepresentation(this StreamFormat exportFormat)
    {
        return exportFormat switch
        {
            StreamFormat.Binary => ".dat",
            StreamFormat.Json => ".json",
            StreamFormat.Xml => ".xml",
            _ => string.Empty
        };
    }
}
