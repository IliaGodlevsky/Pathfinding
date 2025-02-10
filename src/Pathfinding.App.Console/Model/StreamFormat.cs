using System.ComponentModel;

namespace Pathfinding.App.Console.Model;

internal enum StreamFormat
{
    [Description(".dat")] Binary,
    [Description(".json")] Json,
    [Description(".xml")] Xml
}
