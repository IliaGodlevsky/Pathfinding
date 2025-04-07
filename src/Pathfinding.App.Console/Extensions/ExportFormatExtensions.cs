using Pathfinding.App.Console.Models;

namespace Pathfinding.App.Console.Extensions
{
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

        public static int GetOrder(this StreamFormat exportFormat)
        {
            return exportFormat switch
            {
                StreamFormat.Binary => 2,
                StreamFormat.Json => 3,
                StreamFormat.Xml => 4,
                StreamFormat.Csv => 1,
                _ => int.MaxValue
            };
        }
    }
}
