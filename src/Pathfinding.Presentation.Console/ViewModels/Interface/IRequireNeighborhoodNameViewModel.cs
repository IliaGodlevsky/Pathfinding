using Pathfinding.Domain.Enums;

namespace Pathfinding.Presentation.Console.ViewModels.Interface;

internal interface IRequireNeighborhoodNameViewModel
{
    Neighborhoods Neighborhood { get; set; }

    IReadOnlyCollection<Neighborhoods> AllowedNeighborhoods { get; }
}