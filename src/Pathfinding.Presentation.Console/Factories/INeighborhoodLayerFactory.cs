using Pathfinding.Domain.Enums;
using Pathfinding.Service.Layers;

namespace Pathfinding.Presentation.Console.Factories;

public interface INeighborhoodLayerFactory
{
    IReadOnlyCollection<Neighborhoods> AvailableNeighborhoods { get; }

    NeighborhoodLayer Create(Neighborhoods neighborhoods);
}