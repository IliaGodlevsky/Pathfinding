using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireSmoothLevelViewModel
{
    SmoothLevels SmoothLevel { get; set; }
}