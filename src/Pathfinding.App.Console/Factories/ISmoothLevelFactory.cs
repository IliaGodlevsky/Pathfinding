using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.App.Console.Factories;

public interface ISmoothLevelFactory
{
    IReadOnlyCollection<SmoothLevels> Allowed { get; }

    SmoothLayer CreateLayer(SmoothLevels level);
}