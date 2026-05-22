using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRequireSmoothLevelViewModel
{
    SmoothLevels SmoothLevel { get; set; }

    IReadOnlyCollection<SmoothLevels> AllowedLevels { get; }
}