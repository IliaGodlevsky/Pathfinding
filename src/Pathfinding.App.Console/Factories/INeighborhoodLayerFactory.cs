using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;

namespace Pathfinding.App.Console.Factories;

public interface INeighborhoodLayerFactory
{
    IReadOnlyCollection<Neighborhoods> Allowed { get; }

    ILayer CreateNeighborhoodLayer(Neighborhoods neighborhoods);
}