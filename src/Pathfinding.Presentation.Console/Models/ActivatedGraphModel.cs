using Pathfinding.Domain.Enums;
using static Pathfinding.Presentation.Console.ViewModels.ViewModel;

namespace Pathfinding.Presentation.Console.Models;

internal record ActivatedGraphModel(
    ActiveGraph ActiveGraph,
    Neighborhoods Neighborhood,
    SmoothLevels SmoothLevel);