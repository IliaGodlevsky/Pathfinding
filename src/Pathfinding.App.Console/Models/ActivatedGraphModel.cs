using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Data.Pathfinding;
using static Pathfinding.App.Console.ViewModels.ViewModel;

namespace Pathfinding.App.Console.Models;

internal record ActivatedGraphModel(
    ActiveGraph ActiveGraph,
    Neighborhoods Neighborhood,
    SmoothLevels SmoothLevel);