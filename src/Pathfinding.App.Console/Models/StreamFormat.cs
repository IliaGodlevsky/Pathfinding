using System.ComponentModel;

namespace Pathfinding.App.Console.Models;

internal enum StreamFormat
{
    [Description(".dat")] Binary,
    [Description(".json")] Json,
    [Description(".xml")] Xml
}
