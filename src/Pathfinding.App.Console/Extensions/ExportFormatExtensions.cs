using Pathfinding.App.Console.Models;
using System.ComponentModel;

namespace Pathfinding.App.Console.Extensions
{
    internal static class ExportFormatExtensions
    {
        public static string ToExtensionRepresentation(this StreamFormat exportFormat)
        {
            var field = exportFormat.GetType().GetField(exportFormat.ToString());
            var attribute = Attribute.GetCustomAttribute(field, 
                typeof(DescriptionAttribute)) as DescriptionAttribute;
            return attribute?.Description ?? DescriptionAttribute.Default.Description;
        }
    }
}
