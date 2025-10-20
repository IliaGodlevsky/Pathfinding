using Pathfinding.Domain.Core.Enums;
using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Business.Layers;

namespace Pathfinding.App.Console.Factories;

public interface INeighborhoodLayerFactory
{
    IReadOnlyCollection<Neighborhoods> Allowed { get; }

    NeighborhoodLayer CreateNeighborhoodLayer(Neighborhoods neighborhoods);
}