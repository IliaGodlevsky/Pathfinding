using Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;
using Pathfinding.Service.Interface;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class ExtendedMooreNeighborhoodLayer : NeighborhoodLayer
{
    protected override INeighborhood CreateNeighborhood(Coordinate coordinate)
    {
        return new ExtendedMooreNeighborhood(coordinate);
    }
}
