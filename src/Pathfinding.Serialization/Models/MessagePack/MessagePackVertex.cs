using MessagePack;

namespace Pathfinding.Serialization.Models.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal class MessagePackVertex
{
    [Key(0)]
    public int GraphId { get; set; }

    [Key(1)]
    public string Coordinate { get; set; }

    [Key(2)]
    public int Cost { get; set; }

    [Key(3)]
    public bool IsObstacle { get; set; }
}
