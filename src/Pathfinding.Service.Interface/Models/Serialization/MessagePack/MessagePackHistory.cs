using MessagePack;

namespace Pathfinding.Service.Interface.Models.Serialization.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal class MessagePackHistory
{
    [Key(0)]
    public MessagePackGraph Graph { get; set; }

    [Key(1)]
    public IReadOnlyCollection<MessagePackVertex> Vertices { get; set; }

    [Key(2)]
    public IReadOnlyCollection<MessagePackStatistics> Statistics { get; set; }

    [Key(3)]
    public IReadOnlyCollection<MessagePackRange> Range { get; set; }
}
