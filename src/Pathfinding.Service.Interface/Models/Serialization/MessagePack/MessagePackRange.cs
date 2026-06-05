using MessagePack;

namespace Pathfinding.Service.Interface.Models.Serialization.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal class MessagePackRange
{
    [Key(0)]
    public int GraphId { get; set; }

    [Key(1)]
    public string Coordinate { get; set; }
}
