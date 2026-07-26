using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRequireGraphGeneratorViewModel
{
    GraphGenerators Generator { get; set; }

    IReadOnlyCollection<GraphGenerators> AllowedGenerators { get; }
}
