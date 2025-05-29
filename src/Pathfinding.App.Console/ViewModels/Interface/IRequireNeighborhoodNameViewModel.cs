using Pathfinding.Domain.Core.Enums;

namespace Pathfinding.App.Console.ViewModels.Interface;

internal interface IRequireNeighborhoodNameViewModel
{
    Neighborhoods Neighborhood { get; set; }
}