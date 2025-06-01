using Pathfinding.Domain.Interface;
using Pathfinding.Infrastructure.Data.Pathfinding.Neighborhoods;
using Pathfinding.Shared.Primitives;

namespace Pathfinding.Infrastructure.Business.Layers;

public sealed class VonNeumannNeighborhoodLayer : NeighborhoodLayer
{
    protected override INeighborhood CreateNeighborhood(Coordinate coordinate)
    {
        return new VonNeumannNeighborhood(coordinate);
    }
}
