using MessagePack;
using Pathfinding.Domain.Enums;

namespace Pathfinding.Service.Interface.Models.Serialization.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal sealed class MessagePackGraph
{
    [Key(0)]
    public int Id { get; set; }

    [Key(1)]
    public string Name { get; set; }

    [Key(2)]
    public SmoothLevels SmoothLevel { get; set; }

    [Key(3)]
    public Neighborhoods Neighborhood { get; set; }

    [Key(4)]
    public GraphStatuses Status { get; set; }

    [Key(5)]
    public string DimensionSizes { get; set; }

    [Key(6)]
    public int UpperValueOfRange { get; set; }

    [Key(7)]
    public int LowerValueOfRange { get; set; }
}
