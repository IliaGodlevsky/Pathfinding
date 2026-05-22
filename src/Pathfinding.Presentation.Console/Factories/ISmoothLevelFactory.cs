using Pathfinding.Domain.Enums;
using Pathfinding.Service.Layers;

namespace Pathfinding.Presentation.Console.Factories;

public interface ISmoothLevelFactory
{
    IReadOnlyCollection<SmoothLevels> AvailableLevels { get; }

    SmoothLayer Create(SmoothLevels level);
}