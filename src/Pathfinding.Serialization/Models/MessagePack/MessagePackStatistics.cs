using MessagePack;
using Pathfinding.Domain.Enums;

namespace Pathfinding.Serialization.Models.MessagePack;

[MessagePackObject(AllowPrivate = true)]
internal class MessagePackStatistics
{
    [Key(0)]
    public int GraphId { get; set; }

    [Key(1)]
    public Algorithms Algorithm { get; set; }

    [Key(2)]
    public Heuristics? Heuristics { get; set; } = null;

    [Key(3)]
    public double? Weight { get; set; } = null;

    [Key(4)]
    public StepRules? StepRule { get; set; } = null;

    [Key(5)]
    public RunStatuses ResultStatus { get; set; }

    [Key(6)]
    public double Elapsed { get; set; }

    [Key(7)]
    public int Steps { get; set; }

    [Key(8)]
    public double Cost { get; set; }

    [Key(9)]
    public int Visited { get; set; }
}
