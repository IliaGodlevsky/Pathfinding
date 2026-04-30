using Pathfinding.Domain.Core.Enums;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.App.Console.Factories;

public interface INeighborhoodLayerFactory
{
    IReadOnlyCollection<Neighborhoods> AvailableNeighborhoods { get; }

    NeighborhoodLayer CreateNeighborhoodLayer(Neighborhoods neighborhoods);
}