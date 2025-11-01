using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.Service.Interface.Models;

public interface IAlgorithmBuildInfo
{
    Algorithms Algorithm { get; }

    Heuristics? Heuristics { get; }

    double? Weight { get; }

    StepRules? StepRule { get; }

    int? BeamWidth { get; }
}
